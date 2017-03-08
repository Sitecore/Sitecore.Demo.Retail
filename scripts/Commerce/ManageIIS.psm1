Import-Module PKI
Import-Module WebAdministration

function New-AppPool
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$appPoolSettingList,
        [Parameter(Mandatory=$True)][PSCustomObject]$accountSettingList
    )

    begin 
    {
        Write-Verbose "Creating Application Pools"
    }
    process
    {
        Foreach ($appPool in $appPoolSettingList)
        {
            if (Test-IisAppPool -Name $appPool.name)
            {
                Write-Verbose "App Pool already exists: $($appPool.id)"
            }
            else 
            {
                Write-Verbose "Creating App Pool: $($appPool.id)"

                $account = ($accountSettingList | Where { $_.id -eq $appPool.accountId } | Select)
                If ($account -eq $null)
                {
                    Write-Host "Account '$($appPool.accountId)' can't be found in list of supplied accounts when processing application pool '$($appPool.id)'." -ForegroundColor red
                    return 1;
                }

                $secpasswd = ConvertTo-SecureString $account.password -AsPlainText -Force
                $appPoolCredential = New-Object System.Management.Automation.PSCredential ($account.username, $secpasswd)
                Install-IisAppPool -Name $appPool.name -ManagedRuntimeVersion $appPool.managedRuntimeVersion -Credential $appPoolCredential
            }
        }

        return 0;
    }
    end{}
}

function New-Website
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$websiteSettingList,
        [Parameter(Mandatory=$True)][PSCustomObject]$appPoolSettingList
    )

    begin 
    {
        Write-Verbose "Creating Websites"
    }
    process
    {
        Foreach ($website in $websiteSettingList)
        {
            if (Test-IisWebsite -Name $website.siteName)
            {
                Write-Verbose "Website already exists: $($website.id)"
            }
            else 
            {
                Write-Verbose "Creating Website: $($website.id)"

                $appPool = ($appPoolSettingList | Where { $_.id -eq $website.appPoolId } | Select)
                If ($appPool -eq $null)
                {
                    Write-Host "App Pool '$($website.appPoolId)' can't be found in list of supplied appPools. Expected for website '$($website.id)'" -ForegroundColor red
                    return 1;
                }

                $bindingList = @()
                Foreach ($binding in $website.bindings)
                {
                    $bindingList += ,"$($binding.protocol)/$($binding.ipAddress):$($binding.port):$($binding.hostName)"
                }

                Install-IisWebsite -Name $website.siteName -PhysicalPath $website.physicalPath -AppPoolName $appPool.name -Bindings $bindingList
            }
        }

        return 0;
    }
    end
    {
        Write-Verbose "Created Websites"
    }
}

function Set-HostFile
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$hostEntryList
    )

    begin 
    {
        Write-Verbose "Setting Up Host File"
    }
    process
    {
        Foreach ($hostEntry in $hostEntryList)
        {
            Write-Verbose "Setting Host Entry: $($hostEntry.hostName)"

            Set-HostsEntry -IPAddress $hostEntry.ipAddress -HostName $hostEntry.hostName -Description $hostEntry.description
        }

        return 0;
    }
    end
    {
        Write-Verbose "Setting Up Host File Completed"
    }
}

function Remove-Site
{
    param 
    (
        [Parameter(Mandatory=$True)][string]$name
    )

    begin {}
    process
    {
        if (Test-IisWebsite -Name $name)
        {
            $site = Get-IisApplication -SiteName $name
            ForEach ($path in $site.PhysicalPath)
            {
                if (Test-Path $path)
                {
                    Remove-Item -Recurse -Force $path
                }
            }
            Uninstall-IisWebsite -Name $name    
            Write-Verbose "Removed IISSite $name"
        }
        else 
        {
            Write-Verbose "IISSite $name does not exist" 
        }
    }
    end {}
}

function Remove-AppPool
{
    param 
    (
        [Parameter(Mandatory=$True)][string]$name
    )

    begin {}
    process
    {
        if (Test-IisAppPool -Name $name)
        {
            Uninstall-IisAppPool -Name $name
            Write-Verbose "Removed appPool $name"
        }
        else 
        {
            Write-Verbose "appPool $name does not exist"   
        }
    }
    end {}
}

function Test-WebService
{
    param 
    (
        [Parameter(Mandatory=$True)][string]$uri,
        [Parameter(Mandatory=$True)][string]$username,
        [Parameter(Mandatory=$True)][string]$pwd
    )
    begin {}
    process
    {
        [hashtable]$return = @{}
        #$req = [system.Net.WebRequest]::Create($uri)
        $passwd = ConvertTo-SecureString $pwd -AsPlainText -Force
        $credentials = New-Object System.Management.Automation.PSCredential ($username, $passwd);
        $req = New-WebServiceProxy -Uri $uri  -Credential $credentials
        
        try
        {
            $res = $req.GetServiceVersion()
        }
        catch [System.Net.WebException]
        {
            $res = $_.Exception.Response
        }
        return $res;
    }
    end { }
}

function New-Certificate
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$certificateSettingList,
        [Parameter(Mandatory=$True)][PSCustomObject]$installFolderSetting
    )
    begin 
    {
        Write-Verbose "Setting Up Certificates"
    }
    process
    {
        $myCertStoreLocation = "cert:\LocalMachine\My"
        $rootCertStoreLocation = "cert:\LocalMachine\Root"
        $protocol = "https"
        $port = "443"

        Foreach ($certificateSetting in $certificateSettingList)
        {
            Write-Verbose "Setting up certificate $($certificateSetting.dnsName)"
            
            $certificate = Get-ChildItem $myCertStoreLocation | where-object { $_.DnsNameList -like $certificateSetting.dnsName }  | Select-Object -First 1
            If(-not $certificate)
            {
                Write-Verbose "Creating certificate $($certificateSetting.dnsName) in store '$myCertStoreLocation'"
                
                # The self signed certificate command only supports 'Cert:\CurrentUser\My' or 'Cert:\LocalMachine\My'                    
                PKI\New-SelfSignedCertificate -DnsName $certificateSetting.dnsName -CertStoreLocation $myCertStoreLocation -verbose | Write-Verbose
                $certificate = Get-ChildItem $myCertStoreLocation | where-object { $_.DnsNameList -like $certificateSetting.dnsName }  | Select-Object -First 1
      
                If(-not $certificate)
                {
                    Write-Host "Error, unable to create certificate $($certificateSetting.dnsName) in '$myCertStoreLocation'" -ForegroundColor red
                    return 1; 
                }

            }
            Else
            {
                Write-Verbose "Certificate $($certificateSetting.dnsName) already exists in store '$myCertStoreLocation'"
            }

            $rootCertificate = Get-ChildItem $rootCertStoreLocation | where-object { $_.DnsNameList -like $certificateSetting.dnsName }  | Select-Object -First 1
            If(-not $rootCertificate)
            {
                Write-Verbose "Creating certificate $($certificateSetting.dnsName) in store '$rootCertStoreLocation'"
                
                $mypwd = ConvertTo-SecureString -String "sitecore" -Force -AsPlainText
                $filePath = $installFolderSetting.path + "\" + $certificateSetting.DnsName + ".pfx"

                Write-Verbose "Exporting certificate to $($installFolderSetting.path)"
                $certificate | Export-PfxCertificate -FilePath $filePath -Password $mypwd -Verbose | Write-Verbose

                Write-Verbose "Importing certificate $($installFolderSetting.path) to store '$rootCertStoreLocation'"
                Import-PfxCertificate -FilePath $filePath -CertStoreLocation $rootCertStoreLocation -Password $mypwd -Verbose | Write-Verbose

                $rootCertificate = Get-ChildItem $rootCertStoreLocation | where-object { $_.DnsNameList -like $certificateSetting.dnsName }  | Select-Object -First 1
                If(-not $rootCertificate)
                {
                    Write-Host "Error, unable to create certificate $($certificateSetting.dnsName) in '$rootCertStoreLocation'" -ForegroundColor red
                    return 1; 
                }
            }
            Else
            {
                Write-Verbose "Certificate $($certificateSetting.dnsName) already exists in store '$rootCertStoreLocation'"
            }

            # AppId is used as a reference to which application created the binding for audit purposes.
            $applicationId = ([guid]::newguid()).ToString('B')
            
            # From IIS 7, the OS is responsible for SSL to port mappings. The OS doesn't care about what IIS site made the configuration - thats just a visualisation 
            # in IIS Manager (i.e. the link in IIS to the SSL\Port mapping is maintained in IIS metadata). 
            # NOTE: there is potentialy you can have a certificate mapped correctly and not see it in IIS Manager.
            $cmd = netsh http add sslcert hostnameport=$($certificateSetting.dnsName):$port certhash=$($certificate.Thumbprint) appid=$applicationId certstore=My

            if($cmd -Match 'SSL Certificate successfully added')
            {
                Write-Verbose "Certificate successfully bound to host '$($certificateSetting.dnsName)' and port '$port'."
            }
            ElseIf($cmd -Match 'Cannot create a file when that file already exists')
            {
                Write-Verbose "Certificate already bound. Response from command is: '$cmd'"
            }
            Else
            {
                Write-Host "Error, unable to bind certificate to hostname '$($certificateSetting.dnsName)'. Response from command '$cmd'." -ForegroundColor red
                return 1; 
            }

            # Change the IIS metadata so we can see the SSL\Port configuration in IIS Manager
            $bindingInformation = "*:$($port):$($certificateSetting.dnsName)"
            $escaped = [Regex]::Escape($bindingInformation)
            $cmd = invoke-expression "$($env:WINDIR)\system32\inetsrv\Appcmd list site `"$($certificateSetting.siteName)`" /Config"
            If($cmd -Match $escaped)
            {
                Write-Verbose "IIS configuration allready applied."
            }
            Else
            {
                $cmd = invoke-expression "$($env:WINDIR)\system32\inetsrv\Appcmd set site /site.name: `"$($certificateSetting.siteName)`" /+`"bindings.[protocol='https',bindingInformation='$bindingInformation',sslFlags='0']`" /commit:apphost"
                if($cmd -Like "SITE object * changed")
                {
                    Write-Verbose "IIS  successfully bound to host '$($certificateSetting.dnsName)' and port '$port'."
                }
                Else
                {
                    Write-Host "Error, unable to create IIS metadata for binding of certificate '$($certificateSetting.dnsName)'. Response from command '$cmd'." -ForegroundColor red
                    return 1; 
                }
            }
        }

        return 0;
    }
    end { }  
}

Export-ModuleMember New-AppPool, New-Website, Set-HostFile, Remove-Site, Remove-AppPool, Test-WebService, New-Certificate