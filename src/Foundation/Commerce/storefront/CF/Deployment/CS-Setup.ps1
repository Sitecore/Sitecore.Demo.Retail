#Requires -Version 3

param (
		[string] $csSiteName = "CSSolutionStorefrontsite", 
		[string] $runtimeSiteUser = "$($env:userdomain)\$($env:username)", 
		[string] $catalogWSUser = "$($env:userdomain)\$($env:username)", 
		[string] $ordersWSUser = "$($env:userdomain)\$($env:username)", 
		[string] $profilesWSUser = "$($env:userdomain)\$($env:username)", 
		[string] $marketingWSUser = "$($env:userdomain)\$($env:username)",
		[string] $webServiceSiteName = "CSWebServices",
		[string] $webServiceSiteDiskPath = ( Join-Path "c:\inetpub\" -ChildPath $webServiceSiteName),
		[string] $webServicePortNumber = 1012,
		[string] $catalogAppPool = "CSCSCatalogWebService",
		[string] $ordersAppPool = "CSCSOrdersWebService",
		[string] $profilesAppPool = "CSCSProfilesWebService",
		[string] $marketingAppPool = "CSCSMarketingWebService",
	  [string] $catalogAzmanFile
)

if ((Get-Module -ListAvailable CSPS) -eq $null)
{
    Write-Host "Importing CSPS Module using direct path"
    Import-Module "C:\Program Files (x86)\Commerce Server 11\PowerShell\Modules\CSPS\CSPS.psd1"
}
else
{
    Write-Host "Importing CSPS Module"
    Import-Module CSPS
}

function CreateAndRegisterProfileKeys
(
	[parameter(Mandatory = $true)]
	[string] $csSiteName,
	
	[parameter(Mandatory = $false)]
	[string] $outputDir = "."
)
{
	Write-Host "Creating profile keys...";
	Write-Host "Creating file..." -ForegroundColor Green;
	$returnValue = Start-Process -FilePath ( Join-Path $($Env:COMMERCE_SERVER_ROOT) -ChildPath "Tools\ProfileKeyManager.exe") -ArgumentList @( "/kn", "/o `"$($outputDir)\ProfilesKeys.xml`"", "/f" ) -NoNewWindow -Wait -PassThru;
						
	if( $returnValue.ExitCode -ne 0 )
	{
		Write-Error "Program Exit Code was $($returnValue.ExitCode), aborting.`r$($returnValue.StandardError)";
	}						

	Write-Host "Setting 32bit registry..." -ForegroundColor Green;
	$returnValue = Start-Process -FilePath ( Join-Path $($Env:COMMERCE_SERVER_ROOT) -ChildPath "Tools\ProfileKeyManager.exe") -ArgumentList @( "/ke", "/kf `"$($outputDir)\ProfilesKeys.xml`"", "/reg HKEY_LOCAL_MACHINE\SOFTWARE\CommerceServer\Encryption\Keys\$($csSiteName)", "/f" ) -NoNewWindow -Wait -PassThru;
						
	if( $returnValue.ExitCode -ne 0 )
	{
		Write-Error "Program Exit Code was $($returnValue.ExitCode), aborting.`r$($returnValue.StandardError)";
	}
										
	Write-Host "Setting 64bit registry..." -ForegroundColor Green;
	$returnValue = Start-Process -FilePath ( Join-Path $($Env:COMMERCE_SERVER_ROOT) -ChildPath "Tools\ProfileKeyManager.exe") -ArgumentList @( "/ke", "/kf `"$($outputDir)\ProfilesKeys.xml`"", "/reg HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\CommerceServer\Encryption\Keys\$($csSiteName)", "/f" ) -NoNewWindow -Wait -PassThru;
						
	if( $returnValue.ExitCode -ne 0 )
	{
		Write-Error "Program Exit Code was $($returnValue.ExitCode), aborting.`r$($returnValue.StandardError)";
	}
}

# Remove existing site
Remove-CSSite -Name $csSiteName -DeleteDatabases $true -DeleteGlobalResources $true;

# Create Site
New-CSSite $csSiteName;

# Create resources
Write-Host "Creating site resources." -ForegroundColor Green;
Add-CSCatalogResource $csSiteName;
Add-CSInventoryResource $csSiteName;
Add-CSProfilesResource $csSiteName;

# Create profile keys for site
Write-Host "Creating new profile keys." -ForegroundColor Green;
CreateAndRegisterProfileKeys $csSiteName;

# Setting the Commerce Server Display Out-of-Stock items flag
Write-Host "Setting resource properties." -ForegroundColor Green;
Set-CSSiteResourceProperty -Name $csSiteName -Resource "Inventory" -PropertyName "f_display_oos_skus" -PropertyValue $true;
Set-CSSiteResourceProperty -Name $csSiteName -Resource "Inventory" -PropertyName "i_stock_handling" -PropertyValue 1;

# Setup web services
New-CSWebService -Name $csSiteName -Resource Catalog -IISSite $webServiceSiteName;
Grant-CSCatalogWebServicePermissions -File $catalogAzmanFile -Identity $runtimeSiteUser -Role "Administrator";
New-CSWebService -Name $csSiteName -Resource Profiles -IISSite $webServiceSiteName;

# Secure CS databases
Write-Host "Setting up Commerce Server database permissions for web services and site." -ForegroundColor Green;
Grant-CSManagementPermissions -Name $csSiteName -Identity $runtimeSiteUser;
Grant-CSCatalogManagementPermissions -Name $csSiteName -Identity $catalogWSUser;
Grant-CSProfilesManagementPermissions -Name $csSiteName -Identity $profilesWSUser;