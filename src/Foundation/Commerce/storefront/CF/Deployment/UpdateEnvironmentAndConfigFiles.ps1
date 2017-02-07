#Requires -Version 3
# Update the following files:
#   Environment.xml
#   CleanUpSitecoreInstall.ps1
#   Deploy-Env-Common.ps1

# Updated variables
#------------------------------------------------------------------------------
$newDeployConfig="ReleaseDeploy"
$newInstance="CSTest"
#$newHostHeaderName="reference.cstest.com"
$newSitecoreCmsZip="..\Sitecore\Sitecore *.zip"
$newSitecoreLicense="..\Sitecore\license.xml"
$newModuleSrcPath="..\Modules"
$newPackagePath = "..\Packages"

# Environment.xml
#------------------------------------------------------------------------------
$fileName="Environment.xml"
$origFileName="Environment.orig.xml"
$tempInstanceName = "`"" + $newInstance + "`""

Copy-Item  $fileName $origFileName -force
$file=Get-ChildItem $fileName
(Get-Content $file.PSPath) | 
Foreach-Object {$_ -replace "`"CFRefStorefront`"", $tempInstanceName} | 
#Foreach-Object {$_ -replace "cf.reference.storefront.com", $newHostHeaderName} |
Foreach-Object {$_ -replace '\$\(\$DEPLOYMENT_DIRECTORY\)\\packages\\SitecorePackage.\*\\content\\Sitecore\*.zip', $newSitecoreCmsZip} | 
Foreach-Object {$_ -replace '\$\(\$DEPLOYMENT_DIRECTORY\)\\License\\license.xml', $newSitecoreLicense} | 
Foreach-Object {$_ -replace "c:\\CommerceServices", '..'} |
Foreach-Object {$_ -replace "DevDeploy", $newDeployConfig} |
Set-Content $file.PSPath -force

# CleanUpSitecoreInstall.ps1
#------------------------------------------------------------------------------
$fileName="CleanUpSitecoreInstall.ps1"
$origFileName="CleanUpSitecoreInstall.orig.ps1"

Copy-Item  $fileName $origFileName -force
$file=Get-ChildItem $fileName
(Get-Content $file.PSPath) | 
Foreach-Object {$_ -replace "CFRefStorefront", $newInstance} |  
#Foreach-Object {$_ -replace "cf.reference.storefront.com", $newHostHeaderName} |
Set-Content $file.PSPath -force

# Deploy-Env-Common.ps1
#------------------------------------------------------------------------------
$fileName="Deploy-Env-Common.ps1"
$origFileName="Deploy-Env-Common.orig.ps1"

Copy-Item  $fileName $origFileName -force
$file=Get-ChildItem $fileName
(Get-Content $file.PSPath) | 
Foreach-Object {$_ -replace "DevDeploy", $newDeployConfig} | 
Foreach-Object {$_ -replace "CFRefStorefront", $newInstance} | 
#Foreach-Object {$_ -replace "cf.reference.storefront.com", $newHostHeaderName} | 
Foreach-Object {$_ -replace "Zips", "Modules"} |
Foreach-Object {$_ -replace ".\\packages\\Sitecore.Commerce.Connect.*\\content", $newModuleSrcPath} |
Set-Content $file.PSPath -force

# HttpModulesTweak-WebConfig.ps1
#------------------------------------------------------------------------------
$fileName="HttpModulesTweak-WebConfig.ps1"
$origFileName="HttpModulesTweak-WebConfig.orig.ps1"

Copy-Item  $fileName $origFileName -force
$file=Get-ChildItem $fileName
(Get-Content $file.PSPath) | 
Foreach-Object {$_ -replace "CFRefStorefront", $newInstance} | 
Set-Content $file.PSPath -force