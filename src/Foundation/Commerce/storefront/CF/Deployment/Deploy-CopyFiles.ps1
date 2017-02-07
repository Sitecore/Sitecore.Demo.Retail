#Requires -Version 3
. .\Deploy-Env-Common.ps1

#Copy Sitecore Commerce Server Connect Packages and utilities
Write-Host "Copying Sitecore Commerce Server Connect packages and utilities to site..." -ForegroundColor Green; 
Copy-Item $siteUtilitiesDirSrc -destination $siteUtilitiesDirDst -recurse -container -force
Copy-Item $modulesDirSrc -destination $modulesDirDst -force
Copy-Item $advWorksImages -destination $modulesDirDst -force
Copy-Item $helixImages -destination $modulesDirDst -force

IF ($deployConfig -match "DevDeploy")
{
	Copy-Item $packagesAppSrc -destination $packagesDirDst -force
	Copy-Item $packagesMvcSiteSrc -destination $packagesDirDst -force
	Copy-Item $packagesStorefrontCommonSrc -destination $packagesDirDst -force
	Copy-Item $packagesVnextSrc -destination $packagesDirDst -force
}
ELSE
{
	Copy-Item $packagesQAPackages -destination $packagesDirDst -force
	
	# Copy Speak Components
	#Copy-Item $speakComponents -destination $modulesDirDst -force
}

IF (Test-Path $packagesMerchManagerSrc)
{
	Copy-Item $packagesMerchManagerSrc -destination $packagesDirDst -force
}

Write-Host "Copying Sitecore Commerce Server Connect packages and utilities to site complete..." -ForegroundColor Green; 