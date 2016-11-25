#Requires -Version 3
. .\Deploy-Env-Common.ps1

function Unzip-Files($packageName, $zipName)
{
	Write-Host "TEMP: Copying $packageName files" -ForegroundColor Green ; 
	
	$INSTANCE_PATH = $installDir
	$INSTANCE_WEBSITE_PATH = "$INSTANCE_PATH\Website"
	
	$tempPath = "C:\CSFiles_Storefront\temp"
	$zipFile = "$tempPath\$zipName"
	
	# Clean temp directory
	Remove-Item -Recurse -Force $tempPath -ErrorAction Ignore
	New-Item -ItemType directory -Path $tempPath
	
	# Copy update file and rename to zip
	Copy-Item "$installDir\Website\sitecore\admin\Packages\$packageName" -destination $zipFile -force
	
	# Extract package.zip	
	$shell = new-object -com shell.application
	$zip = $shell.NameSpace($zipFile)
	foreach($item in $zip.items())
	{
		$shell.Namespace($tempPath).copyhere($item)
	}	
	
	# Extract sitecore folder
	$file = "$tempPath\package.zip"
	$folder = "addedfiles\sitecore"
	$item = $shell.NameSpace("$file\$folder")
	$shell.Namespace($INSTANCE_WEBSITE_PATH).copyhere($item, 0x14) # 0x14 = Overwrite
}

# Install Packages
Write-Host "Installing packages..." -ForegroundColor Green; 
$urlInstallPackages = $urlBase + "/InstallPackages.aspx"

#Fix for httpModules inside system.web
.\HttpModulesTweak-WebConfig.ps1

IF ($deployConfig -match "DevDeploy")
{
	Write-Host "Installing Sitecore Commerce Server Connect" -ForegroundColor Green; 
    $packagesDevApp = Get-ChildItem "$packagesDirSrc\Sitecore Commerce Server Connect*.update" | %{$_.Name}
	$packageAppPackage = $urlInstallPackages + "?package=" + $packagesDevApp
	Invoke-RestMethod $packageAppPackage -TimeoutSec 2000

	Write-Host "Installing Sitecore Commerce Engine Connect" -ForegroundColor Green; 
	$packageDevVnext = Get-ChildItem "$packagesDirSrc\Sitecore.Commerce.Engine.Connect*.update" | %{$_.Name}
	$packageVnext = $urlInstallPackages + "?package="  + $packageDevVnext	
	Invoke-RestMethod $packageVnext -TimeoutSec 2000
	
	Write-Host "Installing Storefront Common Package" -ForegroundColor Green; 
	$packageStorefrontCommon = Get-ChildItem "$packagesDirSrc\Sitecore.Reference.Storefront.Common*.update" | %{$_.Name}
	$packageStorefrontCommonSite = $urlInstallPackages + "?package=" + $packageStorefrontCommon
	Invoke-WebRequest $packageStorefrontCommonSite -TimeoutSec 2000

	Write-Host "Installing Storefront Package" -ForegroundColor Green; 
	$packageStorefront = Get-ChildItem "$packagesDirSrc\Sitecore.Reference.Storefront.Powered.by.SitecoreCommerce*.update" | %{$_.Name}
	$packageMVCSite = $urlInstallPackages + "?package=" + $packageStorefront
	Invoke-WebRequest $packageMVCSite -TimeoutSec 2000
}
ELSE
{
	# QA Deployments
	$packageQAApp = Get-ChildItem "..\Packages\Sitecore Commerce Server Connect*.update" | %{$_.Name}
	$packageQAMerch = Get-ChildItem "..\Packages\Sitecore Commerce Merchandising Manager*.update" | %{$_.Name}
	$packageQAVnext = Get-ChildItem "..\Packages\Sitecore.Commerce.Engine.Connect*.update" | %{$_.Name}
	$packageQAMvc = Get-ChildItem "..\Packages\Sitecore.Reference.Storefront.Powered.by.SitecoreCommerce*.update" | %{$_.Name}
	$packageQAStorefrontCommon = Get-ChildItem "..\Packages\Sitecore.Reference.Storefront.Common*.update" | %{$_.Name}
	$packageQACOMgr = Get-ChildItem "..\Packages\Sitecore Commerce Customer And Order Manager.*.update" | %{$_.Name}
	$packageQAPPMgr = Get-ChildItem "..\Packages\Sitecore Commerce Pricing And Promotion Manager.*.update" | %{$_.Name}
    $packageQAUXShared = Get-ChildItem "..\Packages\Sitecore Commerce Business Tools Shared.*.update" | %{$_.Name}

	Write-Host "Installing Sitecore Commerce Server Connect" -ForegroundColor Green; 
	$packageAppPackage = $urlInstallPackages + "?package=" + $packageQAApp	
	Invoke-RestMethod $packageAppPackage -TimeoutSec 2000
	
	Write-Host "Installing Sitecore Commerce Engine Connect" -ForegroundColor Green; 
	$packageVnext = $urlInstallPackages + "?package=" + $packageQAVnext
	Invoke-RestMethod $packageVnext -TimeoutSec 2000

	Write-Host "Installing Storefront Common Package" -ForegroundColor Green; 
	$packageStorefrontCommonSite = $urlInstallPackages + "?package=" + $packageQAStorefrontCommon
	Invoke-WebRequest $packageStorefrontCommonSite -TimeoutSec 840

	Write-Host "Installing Storefront" -ForegroundColor Green; 
	$packageMVCSite = $urlInstallPackages + "?package=" + $packageQAMvc
	Invoke-WebRequest $packageMVCSite -TimeoutSec 840    

	Write-Host "Installing UX Shared Package" -ForegroundColor Green; 
	$packageUXShared = $urlInstallPackages + "?package=" + $packageQAUXShared
	Invoke-WebRequest $packageUXShared -TimeoutSec 2000

	Unzip-Files $packageQAUXShared "UXShared.zip"

	Write-Host "Installing Merchandising Manager" -ForegroundColor Green; 
	$packageMerchManager = $urlInstallPackages + "?package=" + $packageQAMerch
	Invoke-WebRequest $packageMerchManager -TimeoutSec 2000

	Unzip-Files $packageQAMerch "Merch.zip"

	Write-Host "Installing C&O Manager" -ForegroundColor Green; 
	$packageCOManager = $urlInstallPackages + "?package=" + $packageQACOMgr
	Invoke-WebRequest $packageCOManager -TimeoutSec 2000

	Unzip-Files $packageQACOMgr "COMgr.zip"

	Write-Host "Installing P&P Manager" -ForegroundColor Green; 
	$packagePPManager = $urlInstallPackages + "?package=" + $packageQAPPMgr
	Invoke-WebRequest $packagePPManager -TimeoutSec 2000

	Unzip-Files $packageQAPPMgr "PPMgr.zip"
}

Write-Host "Installing packages complete..." -ForegroundColor Green; 
