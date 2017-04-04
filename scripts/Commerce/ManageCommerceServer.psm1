$global:encryptionKeyRegistryPath = "HKEY_LOCAL_MACHINE\SOFTWARE\CommerceServer\Encryption\Keys\"

function Install-CS
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$installFolderSettings
    )

    begin 
    {
        Write-Verbose "Installing Commerce Server"
    }
    process
    {
        $installerFileName = ($installFolderSettings.files | Where { $_.id -eq "commerceInstaller" }).fileName
        $installerFilePath = $installFolderSettings.path + "\" + $installerFileName
        $options = "-silent NOCSCONFIG"

        $csInstalled = Get-ChildItem "HKLM:\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall" | ForEach { Get-ItemProperty $_.PSPath } | ? { $_ -match "Commerce Server" } | select

        If ($csInstalled) 
        {
            Write-Verbose "Commerce Server is already installed"
        }
        Else
        {
            $process = Start-Process -FilePath $installerFilePath -ArgumentList $options -NoNewWindow -PassThru -Wait -Verbose
            if ($process.ExitCode -ne 0)
            {
                Write-Host "Error Installing Commerce Server exited with status code $($process.ExitCode)." -ForegroundColor red
                return $process.ExitCode
            }
        }

        return 0
    }
    end{}
}

function Uninstall-CS
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$installFolderSettings
    )

    begin 
    {
        Write-Verbose "Uninstalling Commerce Server"
    }
    process
    {
        $installerFileName = ($installFolderSettings.files | Where { $_.id -eq "commerceInstaller" }).fileName
        $installerFilePath = $installFolderSettings.path + "\" + $installerFileName
        $options = "-silent /uninstall"
        $csInstalled = Get-ChildItem "HKLM:\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall" | ForEach { Get-ItemProperty $_.PSPath } | ? { $_ -match "Commerce Server" } | select

        If ($csInstalled) 
        {
            $process = Start-Process -FilePath $installerFilePath -ArgumentList $options -Wait -Passthru
            if ($process.ExitCode -ne 0)
            {
                Write-Host "Error uninstalling Commerce Server exited with status code $($process.ExitCode)." -ForegroundColor red
                return $process.ExitCode
            }
        }
        Else
        {
            Write-Verbose "Commerce Server is not currently installed."
        }
    }
    end{}
}

function Copy-RegistryEnvironmentValuesLocally 
{
    $locations = 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
                 'HKCU:\Environment'

    $locations | ForEach-Object {
        $regValues = Get-Item $_
        $regValues.GetValueNames() | ForEach-Object {
            $regValueName  = $_
            $regValueData = $regValues.GetValue($regValueName)

            if ($userLocation -and $regValueName -ieq 'PATH') {
                $Env:Path += ";$regValueData"
            } else {
                Set-Item -Path Env:\$regValueName -Value $regValueData
            }
        }

        $userLocation = $true
    }
}

function Enable-CS
{
    param 
    (
        [Parameter(Mandatory=$True)][string]$path,
        [Parameter(Mandatory=$True)][PSCustomObject]$csConfigSetting,
        [Parameter(Mandatory=$True)][PSCustomObject]$databaseSettingList,
        [Parameter(Mandatory=$True)][PSCustomObject]$accountSettingList
    )

    begin 
    {
        Write-Verbose "Configuring Commerce Server"
    }
    process
    {
        Copy-RegistryEnvironmentValuesLocally

        $adminDatabaseSetting = ($databaseSettingList | Where { $_.id -eq $csConfigSetting.adminDatabaseId })
        $stagingServiceAccountSetting = ($accountSettingList | Where { $_.id -eq $csConfigSetting.stagingServiceAccountId })
        Set-CSConfigurationFile -path $path -csConfigSetting $csConfigSetting -adminDatabaseSetting $adminDatabaseSetting -stagingServiceAccountSetting $stagingServiceAccountSetting
        
        $cmd = $env:COMMERCE_SERVER_ROOT + "CSConfig.exe"
        $configfile = "$path\csconfig.xml"
        $logfile = "$path\csconfig.log"
        $options = "/l $logfile /s $configfile" # /u - to remove, /f - to reapply 

        Write-Verbose "Initiating Commerce Server Configuration"

        # Won't do anything if the site is already configured, unless option /f is specified 
        $process = Start-Process -FilePath $cmd -ArgumentList $options -NoNewWindow -PassThru -Wait -Verbose
        
        if ($process.ExitCode -ne 0)
        {
            Write-Host "Error configuring Commerce Server exited with status code $($process.ExitCode). Please check the log file for details: $logfile" -ForegroundColor red
            return $process.ExitCode
        }
        Else
        {
            Write-Verbose "Commerce Server Configuration Successful"
        }

        return 0
    }
    end{}
}
 
function Set-CSConfigurationFile
{
    param
    (
        [Parameter(Mandatory=$True)][string]$path,
        [Parameter(Mandatory=$True)][PSCustomObject]$csConfigSetting,
        [Parameter(Mandatory=$True)][PSCustomObject]$adminDatabaseSetting,
        [Parameter(Mandatory=$True)][PSCustomObject]$stagingServiceAccountSetting
    )

    begin 
    {
        Write-Verbose "Creating Commerce Server Configuring File"
    }
    process
    {
        # Example: http://commercesdn.sitecore.net/SCpbCS81/SitecoreCommerceDeploymentGuide/en-us/c_CommerceServerConfiguration.html
        # Set the File Name
        $filePath = "$path\csconfig.xml"
     
        # Create The Document
        $XmlWriter = New-Object System.XMl.XmlTextWriter($filePath,$Null)
     
        # Set The Formatting
        $xmlWriter.Formatting = "Indented"
        $xmlWriter.Indentation = "4"
     
        # Write the XML Decleration
        $xmlWriter.WriteStartDocument()
     
        # Set the XSL
        #$XSLPropText = "type='text/xsl' href='style.xsl'"
        #$xmlWriter.WriteProcessingInstruction("xml-stylesheet", $XSLPropText)
     
        # Write Root Element
        $xmlWriter.WriteStartElement("Configuration")

            # Write the Document
            $xmlWriter.WriteStartElement("SQL")
                $XmlWriter.WriteAttributeString("id", $adminDatabaseSetting.Id)
                $xmlWriter.WriteElementString("Server", $adminDatabaseSetting.server)
                $xmlWriter.WriteElementString("Database", $adminDatabaseSetting.name)
                $xmlWriter.WriteElementString("WindowsSecurity", $csConfigSetting.windowsSecurity)
                $xmlWriter.WriteElementString("UserName", $csConfigSetting.username)
                $xmlWriter.WriteElementString("Password", $csConfigSetting.password)
            $xmlWriter.WriteEndElement() # <-- Closing SQL

            $xmlWriter.WriteStartElement("VirtualDirectory")
                $XmlWriter.WriteAttributeString("ID", "Publishing")
                $XmlWriter.WriteAttributeString("Create", "True")
            $xmlWriter.WriteEndElement() # <-- Closing VirtualDirectory

            $xmlWriter.WriteStartElement("NTService")
                $XmlWriter.WriteAttributeString("ID", "StagingService")
                $XmlWriter.WriteElementString("UserName", $stagingServiceAccountSetting.username)
                $XmlWriter.WriteElementString("Domain", $stagingServiceAccountSetting.domain)
                $XmlWriter.WriteElementString("Password", $stagingServiceAccountSetting.password)
            $xmlWriter.WriteEndElement() # <-- Closing NTService

        # Write Close Tag for Root Element
        $xmlWriter.WriteEndElement() # <-- Closing RootElement
     
        # End the XML Document
        $xmlWriter.WriteEndDocument()
     
        # Finish The Document
        $xmlWriter.Finalize
        $xmlWriter.Flush()
        $xmlWriter.Close()
    }
    end{}
}
function Fix-AuthorizationStores
{
    param 
    (
	     [Parameter(Mandatory=$True)][PSCustomObject]$csSiteSetting,
		 [Parameter(Mandatory=$True)][PSCustomObject]$accountSettingList,
         [Parameter(Mandatory=$True)][PSCustomObject]$appPoolSettingList,
		 [Parameter(Mandatory=$True)][PSCustomObject]$websiteSettingList
   
	)
	begin
	{
		Write-Verbose "Adding Catalog Scope"
	}
	process
	{
	
	
		$csServicesWebsiteSetting = ($websiteSettingList | Where { $_.id -eq $csSiteSetting.csServicesWebsiteId} | Select)
        If ($csServicesWebsiteSetting -eq $null) { Write-Host "Can't derive website from WebsiteId '$($csSiteSetting.csServicesWebsiteId)' when processing CS Site '$($csSiteSetting.name)'." -ForegroundColor red; return 1; }
		
		$catalogAppPoolSetting = ($appPoolSettingList | Where { $_.id -eq $csSiteSetting.catalogAppPoolId } | Select)
		$catalogAccountSetting = ($accountSettingList | Where { $_.id -eq $catalogAppPoolSetting.accountId } | Select)
        If ($catalogAccountSetting -eq $null) { Write-Host "Can't derive application pool identity from AppPoolId '$($csSiteSetting.catalogAppPoolId)' when processing CS Site '$($csSiteSetting.name)'." -ForegroundColor red; return 1; }
        
        $profilesAppPoolSetting = ($appPoolSettingList | Where { $_.id -eq $csSiteSetting.profilesAppPoolId } | Select)
        $profilesAccountSetting = ($accountSettingList | Where { $_.id -eq $profilesAppPoolSetting.accountId } | Select)
        If ($profilesAccountSetting -eq $null) { Write-Host "Can't derive application pool identity from AppPoolId '$($csSiteSetting.profilesAppPoolId)' when processing CS Site '$($csSiteSetting.name)'." -ForegroundColor red; return 1; }

		
		$catalogAzmanFile = $csServicesWebsiteSetting.physicalPath + "\" + $csSiteSetting.name + "_CatalogWebService\CatalogAuthorizationStore.xml"        
        $profilesAzmanFile = $csServicesWebsiteSetting.physicalPath + "\" + $csSiteSetting.name + "_ProfilesWebService\ProfilesAuthorizationStore.xml" 
		
		& 'C:\Program Files (x86)\Commerce Server 11\Tools\CreateCatalogAuthorizationStore.exe' $csSiteSetting.name $catalogAzmanFile 
		
        Grant-CSCatalogWebServicePermissions -File $catalogAzmanFile -Identity $catalogAccountSetting.username -Role "Administrator" | Write-Verbose;
        Grant-CSProfilesWebServicePermissions –File $profilesAzmanFile -Identity $profilesAccountSetting.username -Role "ProfileAdministrator" | Write-Verbose;
		$currentUser = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
		
		Grant-CSCatalogWebServicePermissions -File $catalogAzmanFile -Identity $currentUser -Role "Administrator" | Write-Verbose;
        Grant-CSProfilesWebServicePermissions –File $profilesAzmanFile -Identity $currentUser -Role "ProfileAdministrator" | Write-Verbose;
		
		
		
		return 0;
	}
	end
	{
		Write-Verbose "Added Authorization Scope for site: $($csSiteSetting.name)"
	}
}

function New-CSWebsite
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$csSiteSetting,
        [Parameter(Mandatory=$True)][PSCustomObject]$accountSettingList,
        [Parameter(Mandatory=$True)][PSCustomObject]$appPoolSettingList,
        [Parameter(Mandatory=$True)][PSCustomObject]$websiteSettingList,
        [Parameter(Mandatory=$True)][PSCustomObject]$installFolderSetting,
        [Parameter(Mandatory=$True)][PSCustomObject]$databaseSettingList
    )

    begin 
    {
        Write-Verbose "Creating Site: $($csSiteSetting.name)"
    }
    process
    {
        $csServicesWebsiteSetting = ($websiteSettingList | Where { $_.id -eq $csSiteSetting.csServicesWebsiteId} | Select)
        If ($csServicesWebsiteSetting -eq $null) { Write-Host "Can't derive website from WebsiteId '$($csSiteSetting.csServicesWebsiteId)' when processing CS Site '$($csSiteSetting.name)'." -ForegroundColor red; return 1; }

        $runtimeAppPoolSetting = ($appPoolSettingList | Where { $_.id -eq $csSiteSetting.runTimeAppPoolId } | Select)
        $runtimeAccountSetting = ($accountSettingList | Where { $_.id -eq $runtimeAppPoolSetting.accountId } | Select)
        If ($runtimeAccountSetting -eq $null) { Write-Host "Can't derive application pool identity from AppPoolId '$($csSiteSetting.runTimeAppPoolId)' when processing CS Site '$($csSiteSetting.name)'." -ForegroundColor red; return 1; }
        
        $catalogAppPoolSetting = ($appPoolSettingList | Where { $_.id -eq $csSiteSetting.catalogAppPoolId } | Select)
        $catalogAccountSetting = ($accountSettingList | Where { $_.id -eq $catalogAppPoolSetting.accountId } | Select)
        If ($catalogAccountSetting -eq $null) { Write-Host "Can't derive application pool identity from AppPoolId '$($csSiteSetting.catalogAppPoolId)' when processing CS Site '$($csSiteSetting.name)'." -ForegroundColor red; return 1; }
        
        $profilesAppPoolSetting = ($appPoolSettingList | Where { $_.id -eq $csSiteSetting.profilesAppPoolId } | Select)
        $profilesAccountSetting = ($accountSettingList | Where { $_.id -eq $profilesAppPoolSetting.accountId } | Select)
        If ($profilesAccountSetting -eq $null) { Write-Host "Can't derive application pool identity from AppPoolId '$($csSiteSetting.profilesAppPoolId)' when processing CS Site '$($csSiteSetting.name)'." -ForegroundColor red; return 1; }

        # Fully Qualified Domain Name
        $runtimeFQDN = $runtimeAccountSetting.Domain + "\" + $runtimeAccountSetting.username
        $catalogFQDN = $catalogAccountSetting.Domain + "\" + $catalogAccountSetting.username 
        $profilesFQDN = $profilesAccountSetting.Domain + "\" + $profilesAccountSetting.username 

        $catalogDatabaseSetting = ($databaseSettingList | Where { $_.id -eq $csSiteSetting.catalogDatabaseId })
        $profilesDatabaseSetting = ($databaseSettingList | Where { $_.id -eq $csSiteSetting.profileDatabaseId })
        
        New-CSSite -Name $csSiteSetting.name | Write-Verbose;

        Write-Verbose "Creating site resources.";
        Add-CSCatalogResource  -Name $csSiteSetting.name -DatabaseName $catalogDatabaseSetting.name | Write-Verbose;
        Add-CSInventoryResource -Name $csSiteSetting.name -DatabaseName $catalogDatabaseSetting.name | Write-Verbose;
        Add-CSProfilesResource -Name $csSiteSetting.name -DatabaseName $profilesDatabaseSetting.name | Write-Verbose;

        # Setting the Commerce Server to Display Out-of-Stock items and also enable pre and back order capability
        Write-Verbose "Setting resource properties";
        Set-CSSiteResourceProperty -Name $csSiteSetting.name -Resource "Inventory" -PropertyName "f_display_oos_skus" -PropertyValue $true | Write-Verbose;
        Set-CSSiteResourceProperty -Name $csSiteSetting.name -Resource "Inventory" -PropertyName "i_stock_handling" -PropertyValue 1 | Write-Verbose;

        Write-Verbose "Creating Web Services";
        New-CSWebService -Name $csSiteSetting.name -Resource Catalog -IISSite $csServicesWebsiteSetting.siteName -AppPool $catalogAppPoolSetting.name -Identity $catalogAccountSetting.username -Password $catalogAccountSetting.password | Write-Verbose;
        New-CSWebService -Name $csSiteSetting.name -Resource Profiles -IISSite $csServicesWebsiteSetting.siteName -AppPool $profilesAppPoolSetting.name -Identity $profilesAccountSetting.username -Password $profilesAccountSetting.password | Write-Verbose;

        Write-Verbose "Setting Web Service Permissions";
        $catalogAzmanFile = $csServicesWebsiteSetting.physicalPath + "\" + $csSiteSetting.name + "_CatalogWebService\CatalogAuthorizationStore.xml"        
        $profilesAzmanFile = $csServicesWebsiteSetting.physicalPath + "\" + $csSiteSetting.name + "_ProfilesWebService\ProfilesAuthorizationStore.xml"        
        Grant-CSCatalogWebServicePermissions -File $catalogAzmanFile -Identity $catalogAccountSetting.username -Role "Administrator" | Write-Verbose;
        Grant-CSProfilesWebServicePermissions –File $profilesAzmanFile -Identity $profilesAccountSetting.username -Role "ProfileAdministrator" | Write-Verbose;

        Write-Verbose "Setting Database Permissions"
        Grant-CSManagementPermissions -Name $csSiteSetting.name -Identity $runtimeFQDN | Write-Verbose;
        Grant-CSCatalogManagementPermissions -Name $csSiteSetting.name -Identity $catalogFQDN | Write-Verbose;
        Grant-CSProfilesManagementPermissions -Name $csSiteSetting.name -Identity $profilesFQDN | Write-Verbose;

        If ((New-CSProfileKey -pathToStoreKeyFile $installFolderSetting.path -siteName $csSiteSetting.name -Verbose) -ne 0) { Return 1 }
        
        $profilesWebServiceConfigFilePath = $csServicesWebsiteSetting.physicalPath + "\" + $csSiteSetting.name + "_ProfilesWebService\Web.config"
        If ((Set-CSProfileKeyInProfilesWebService -profilesWebServiceConfigFilePath $profilesWebServiceConfigFilePath -siteName $csSiteSetting.name -Verbose) -ne 0) { Return 1 }

        return 0;
    }
    end
    {
       Write-Verbose "Created site: $($csSiteSetting.name)"
    }
}

function Remove-CSWebsite
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$csSiteSetting
    )

    begin 
    {
        Write-Verbose "Removing Commerce Server Site: $($csSiteSetting.name)"
    }
    process
    {
        Remove-CSSite -Name $csSiteSetting.name  -DeleteDatabases $true -DeleteGlobalResources $true | Write-Verbose;
    }
    end{}
}

function New-CSProfileKey
{
    param 
    (
        [Parameter(Mandatory=$True)][String]$pathToStoreKeyFile,
        [Parameter(Mandatory=$True)][String]$siteName
    )

    begin 
    {
        Write-Verbose "Creating Commerce Server Profile Keys"
    }
    process
    {
        $cmd = $env:COMMERCE_SERVER_ROOT + "tools\ProfileKeyManager.exe"
        $encryptionKeyFilePath = $pathToStoreKeyFile + "\ProfileEncryptionKeys.xml"
        
        if (Test-Path ($encryptionKeyFilePath)) 
        {
            Write-Verbose "File with encryption keys already exists: $encryptionKeyFilePath"
        }
        else
        {
            Write-Verbose "Generating file with encryption keys: $encryptionKeyFilePath"
            $options = "/kn /o $encryptionKeyFilePath /f"

            $process = Start-Process -FilePath $cmd -ArgumentList $options -NoNewWindow -PassThru -Wait -Verbose
            if ($process.ExitCode -ne 0)
            {
                Write-Host "Error generating Commerce Server profile keys, exited with status code $($process.ExitCode)." -ForegroundColor red
                return $process.ExitCode
            }
        }

        Write-Verbose "Saving encryption keys to the registry"
        $options = "/ke /kf $encryptionKeyFilePath /reg $global:encryptionKeyRegistryPath$siteName /f"
        
        $process = Start-Process -FilePath $cmd -ArgumentList $options -NoNewWindow -PassThru -Wait -Verbose
        if ($process.ExitCode -ne 0)
        {
            Write-Host "Error adding Commerce Server profile keys to the registry, exited with status code $($process.ExitCode)." -ForegroundColor red
            return $process.ExitCode
        }

        return 0
    }
    end{}
}

function Set-CSProfileKeyInProfilesWebService
{
    param 
    (
        [Parameter(Mandatory=$True)][String]$profilesWebServiceConfigFilePath,
        [Parameter(Mandatory=$True)][String]$siteName
    )

    begin 
    {
        Write-Verbose "Setting Profile Key References In Profiles WebServices"
    }
    process
    {
        $doc = (Get-Content $profilesWebServiceConfigFilePath) -as [Xml]
        if ($doc -eq $null) 
        {
            Write-Verbose "Profiles Webservice can't be found at path: $profilesWebServiceConfigFilePath"
            return 1;
        }
        
        $root = $doc.get_DocumentElement();
        $profilesWebServiceNode = $root.CommerceServer.profilesWebService;
        
        $regKeyPath = $global:encryptionKeyRegistryPath + $siteName
        $profilesWebServiceNode.SetAttribute("publicKey", "registry:$regKeyPath,PublicKey");
        $profilesWebServiceNode.SetAttribute("privateKey1", "registry:$regKeyPath,PrivateKey");
        $profilesWebServiceNode.SetAttribute("keyIndex", "1");

        $doc.Save($profilesWebServiceConfigFilePath)

        return 0;
    }
    end{}
}

function Import-CSSiteData
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$csSiteSetting,
        [Parameter(Mandatory=$True)][PSCustomObject]$csResourceFolderSetting
    )

    begin 
    {
        Write-Verbose "Importing Commerce Server Site Data"
    }
    process
    {
        $csCatalogFile = ($csResourceFolderSetting.files | Where { $_.id -eq "catalog" } | Select)
        If ($csCatalogFile -eq $null) { Write-Host "Expected Commerce Server catalog resource" -ForegroundColor red; return 1; } 

        $csInventoryFile = ($csResourceFolderSetting.files | Where { $_.id -eq "inventory" } | Select)
        If ($csInventoryFile -eq $null) { Write-Host "Expected Commerce Server inventory resource" -ForegroundColor red; return 1; } 
        
        $csProfilesFile = ($csResourceFolderSetting.files | Where { $_.id -eq "profiles" } | Select)
        If ($csProfilesFile -eq $null) { Write-Host "Expected Commerce Server profiles resource" -ForegroundColor red; return 1; } 
        
        Import-CSCatalog      -Name $csSiteSetting.name -File "$($csResourceFolderSetting.path)\$($csCatalogFile.filename)" -ImportSchemaChanges $true -Mode Full
        Import-CSInventory    -Name $csSiteSetting.name -File "$($csResourceFolderSetting.path)\$($csInventoryFile.filename)" -ImportSchemaChanges $true -Mode Full
        Import-CSProfiles     -Name $csSiteSetting.name -File "$($csResourceFolderSetting.path)\$($csProfilesFile.filename)"

        return 0;
    }
    end{}
}

function Test-CSWebservices
{
    [CmdletBinding()]
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$csSiteSetting,
        [Parameter(Mandatory=$True)][PSCustomObject]$websiteSettingList,
        [Parameter(Mandatory=$True)][PSCustomObject]$appPoolSettingList,
        [Parameter(Mandatory=$True)][PSCustomObject]$accountSettingList
    )
    begin {}
    process
    {
        $services =	"CatalogWebService", "ProfilesWebService"

        $csServicesWebsiteSetting = ($websiteSettingList | Where { $_.id -eq $csSiteSetting.csServicesWebsiteId} | Select)
        If ($csServicesWebsiteSetting -eq $null) { Write-Host "Can't derive website from WebsiteId '$($csSiteSetting.csServicesWebsiteId)' when processing CS Site '$($csSiteSetting.name)'." -ForegroundColor red; return 1; }

        $csServicesBindingSetting = $csServicesWebsiteSetting.bindings | Select -First 1
        If ($csServicesBindingSetting -eq $null) { Write-Host "Website '$($csServicesWebsiteSetting.id)' has no specified binding." -ForegroundColor red; return 1; }
        
        $csServicesAppPoolSetting = ($appPoolSettingList | Where { $_.id -eq $csServicesWebsiteSetting.appPoolId } | Select)
        If ($csServicesAppPoolSetting -eq $null) { Write-Host "Can't derive application pool from AppPoolId '$($csSiteSetting.runTimeAppPoolId)' when processing CS Site '$($csSiteSetting.name)'." -ForegroundColor red; return 1; }
        
        $csServicesAccountSetting = ($accountSettingList | Where { $_.id -eq $csServicesAppPoolSetting.accountId } | Select)
        If ($csServicesAccountSetting -eq $null) { Write-Host "Can't derive account from AppPoolId '$($csSiteSetting.runTimeAppPoolId)' when processing CS Site '$($csSiteSetting.name)'." -ForegroundColor red; return 1; }

        ForEach ($service in $services)
        {
            $name = $csServicesBindingSetting.protocol + "://" + $csServicesBindingSetting.hostName + ":" + $csServicesBindingSetting.port + "/" + $csSiteSetting.name + "_" + $service + "/" + $service + ".asmx?WSDL"
            $result = ManageIIS\Test-WebService -uri $name -username $csServicesAccountSetting.username -pwd $csServicesAccountSetting.password 
            
            if ($result)
            {
                $version = "" + $result.MajorVersion + "." + $result.MinorVersion
                Write-Host "testing $service --> version: $version" -foregroundcolor green
            }
            else 
            {
                Write-Host $result.code
                Write-Host "testing $service, result: $status" -foregroundcolor red
                return 1;
            }
        }

        return 0;
    }
    end { }
}

Export-ModuleMember Install-CS, Uninstall-CS, Enable-CS, New-CSWebsite, Remove-CSWebsite, New-CSProfileKey, Import-CSSiteData, Test-CSWebservices, Fix-AuthorizationStores