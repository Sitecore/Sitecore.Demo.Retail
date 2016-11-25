#Requires -Version 2
$ErrorActionPreference = "Stop";
Set-PSDebug -Strict

trap
{
	Write-Host "Error: $($_.Exception.GetType().FullName)" -ForegroundColor Red ; 
	Write-Host $_.Exception.Message; 
	Write-Host $_.Exception.StackTrack;
	break;
}

function IIS-ConfigureWebConfigDev
{
	param
	(
		[String]$WebConfigPath=$(throw 'Parameter -WebConfigPath is missing!')
	)
	$xml = [xml](get-content $WebConfigPath);
    $xml.Save($WebConfigPath + "." + (Get-Date –f yyyyMMddHHmmss) + ".bak");

	$root = $xml.get_DocumentElement();
    $root."system.web".customErrors.mode = "Off";
	
	# Sharepoint dev
	if( $root.SharePoint.SafeMode -ne $null )
	{
    	$root.SharePoint.SafeMode.CallStack = "True";
	    $root.SharePoint.SafeMode.AllowPageLevelTrace = "True";
	}

	$xml.Save( $WebConfigPath );
}

function IIS-CreateWebApplication
{
	PARAM
	(	
		[String]$WebSiteName=$(throw 'Parameter -WebSiteName is missing!'),
		[String]$WebApplicationName=$(throw 'Parameter -WebApplicationName is missing!'),
		[String]$PhysicalPath=$(throw 'Parameter -PhysicalPath is missing!'),
		[String]$AppPool=$(throw 'Parameter -AppPool is missing!')
		
	)
	Invoke-Expression -Command "& $($env:systemroot)\system32\inetsrv\APPCMD.exe add app /site.Name:`"$($WebSiteName)`" /path:`"/$($WebApplicationName)`" /physicalPath:`"$($PhysicalPath)`" ";
	Invoke-Expression -Command ("& $($env:systemroot)\system32\inetsrv\APPCMD.exe set app `"$($WebSiteName)/$($WebApplicationName)`" /applicationPool:$($AppPool) ");
}

function Import-IIS7PfxCertificate
(
	[String]$certPath,
	[String]$certRootStore = “LocalMachine”,
	[String]$certStore = “My”,
	[String]$pfxPass
)    
{
		$pfx = new-object System.Security.Cryptography.X509Certificates.X509Certificate2;
  
	if ($pfxPass -eq $null)
	{
	   	$pfxPass = read-host "Enter the pfx password" -assecurestring;
	}

	Write-Host "Importing certificate $($certPath)";
	$pfx.Import("$certPath", $pfxPass, [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable -bor [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::PersistKeySet);

	Write-Host "Adding certificate to store $($certRootStore)\$($certStore)";
	$store = new-object System.Security.Cryptography.X509Certificates.X509Store($certStore,$certRootStore);
	$store.open("MaxAllowed");
	$store.add($pfx);
	$store.close();
}

function Create-SelfSignedCert()
{
	PARAM
	(
		[String]$Name=$(throw 'Parameter -Name is missing!')
	)

	Write-Host "Creating a new self-signed cert called $($Name)" -ForegroundColor Green;
	$returnValue = Start-Process -FilePath "$($MAKE_CERT)" -ArgumentList @("/r", "-pe", "-n `"CN=$($Name)`"", "-b 01/01/2010", "-e 09/22/2020", "-eku 1.3.6.1.5.5.7.3.1", "-ss my", "-sr localMachine", "-sky exchange", "-sp `"Microsoft RSA SChannel Cryptographic Provider`"", "-sy 12") -NoNewWindow -Wait -PassThru;
	
	if( $returnValue.ExitCode -ne 0 )
	{
		Write-Error "Program Exit Code was $($returnValue.ExitCode), aborting.`r$($returnValue.StandardError)";
	}    
}

function Execute-HTTPGetCommand
(
	[string]$Url,
	[string]$UserName,
	[string]$Password,
	[switch]$PassThru
)

{
	[System.Net.HttpWebRequest]$webRequest = [System.Net.WebRequest]::Create($Url);
	$webRequest.ServicePoint.Expect100Continue = $false;
	$webRequest.Timeout = [System.Threading.Timeout]::Infinite;
	
	if( $UserName )
	{
		$webRequest.Credentials = New-Object System.Net.NetworkCredential -ArgumentList $UserName, $Password;
	}
	else
	{
		$webRequest.Credentials = [System.Net.CredentialCache]::DefaultNetworkCredentials;
	}
	
	$webRequest.PreAuthenticate = $true;
	$webRequest.Method = [System.Net.WebRequestMethods+Http]::Get;

	[System.Net.HttpWebResponse]$resp = $webRequest.GetResponse();
    $rs = $resp.GetResponseStream();
    [System.IO.StreamReader]$sr = New-Object System.IO.StreamReader -argumentList $rs;
    [string]$results = $sr.ReadToEnd();
	$sr.Close();
	$resp.Close();
	
	if( $PassThru )
	{
    	return $results;
	}
 }

function Assign-AppPoolToSite
(
	[Parameter(Mandatory=$true)][System.Xml.XmlElement]$configuration,
	[Parameter(Mandatory=$true)][string] $websiteAppPool,
	[Parameter(Mandatory=$true)][string] $websiteIdentity,
	[bool]$isVirtualDirectory = $false
)
{
	if( $configuration.IIS.ApplicationPools -and $configuration.IIS.ApplicationPools.ApplicationPool ){

		$appPool = $configuration.IIS.ApplicationPools.ApplicationPool | where { $_.name -ieq "$($websiteAppPool)" };

		if($appPool.name -ne $null) {
			$appPoolInstance = Get-ChildItem "IIS:\AppPools" | where { $_.Name -eq $($appPool.fullName) };

			if($appPoolInstance -eq $null){
				Write-Host "App Pool $($appPool.fullName) does not exist, creating...";

				$appPoolInstance = New-Item "IIS:\AppPools\$($appPool.fullName)";
				$frameworkSet = $false
				
				#set up the framework version before setting up the user account 
				if($appPool.framework -ne $null -and $appPool.framework -ne "") {
					Write-Host "Setting the framework version to be $($appPool.framework)...";
                    $appPoolInstance.managedRuntimeVersion = "$($appPool.framework)";
					$frameworkSet = $true
                    
					#don't set the framework version yet, lets do it all in one set-item action
				}

				$userAccountInstance = $configuration.UserAccounts.UserAccount | where { $_.identity -ieq "$($appPool.userAccountIdentity)" };

				if($userAccountInstance -ne $null){
                    $appPoolInstance.processModel.identityType = 3;
                    $appPoolInstance.processModel.userName = "$($userAccountInstance.domain)\$($userAccountInstance.username)";
                    $appPoolInstance.processModel.password = $userAccountInstance.password;
                    $appPoolInstance | Set-Item;

                    Set-ItemProperty "IIS:\Sites\$($websiteIdentity)" -name applicationPool -value $($appPool.fullName);
					
					if($isVirtualDirectory -eq $false){
						Set-ItemProperty "IIS:\Sites\$($websiteIdentity)" -name ApplicationDefaults.applicationPool -value $($appPool.fullName);
					}                
				}
				else{
					if($frameworkSet){
						# just in case we have a framework version set but no user info 
						$appPoolInstance | Set-Item
					}
					Write-Host "User account $($appPool.userAccountIdentity) configuration cannot be found for app pool $($appPool.fullName)" -ForegroundColor Red;
				}
			}
			else{
				Write-Host "App Pool $($appPool.fullName) exists...";

				Set-ItemProperty "IIS:\Sites\$($websiteIdentity)" -name applicationPool -value $appPoolInstance.name;
				
				if($isVirtualDirectory -eq $false) {
					Set-ItemProperty "IIS:\Sites\$($websiteIdentity)" -name ApplicationDefaults.applicationPool -value $appPoolInstance.name;
				}
			}
		}
		else{
			Write-Host "App Pool $($websiteAppPool) configuration cannot be found" -ForegroundColor Red;
		}
	}
	else{
		Write-Host "There is no app pool section definined in the file." -ForegroundColor Red;
	}
}