#Requires -Version 3
. .\Deploy-Env-Common.ps1

# Install Modules (Sitecore Commerce Connect)
Write-Host "Installing modules..." -ForegroundColor Green ; 

[array]$modules = Get-ChildItem $modulesDirSrc -filter "*.zip" | % {$_.name}
Write-Host "Installing module: " $modules[0] -ForegroundColor Green ; 
$urlInstallModules = $urlBase + "/InstallModules.aspx?modules=" + $modules[0]
Write-Host $urlInstallModules
Invoke-RestMethod $urlInstallModules -TimeoutSec 420 

$moduleToInstall = "Adventure Works Images.zip"

Write-Host "Installing module: " $moduleToInstall -ForegroundColor Green ; 
$urlInstallModules = $urlBase + "/InstallModules.aspx?modules=" + $moduleToInstall
Write-Host $urlInstallModules
Invoke-RestMethod $urlInstallModules -TimeoutSec 420 

$moduleToInstall = Get-ChildItem -Path ( Join-Path $modulesDirDst -ChildPath "\Sitecore.Helix.Images-*.zip" ) | %{$_.Name}

Write-Host "Installing module: " $moduleToInstall -ForegroundColor Green ; 
$urlInstallModules = $urlBase + "/InstallModules.aspx?modules=" + $moduleToInstall
Write-Host $urlInstallModules
Invoke-RestMethod $urlInstallModules -TimeoutSec 420 

# Enabled for QA deployments only, install SPEAK packages
#IF ($deployConfig -notmatch "DevDeploy")
#{
#	$moduleToInstall = Get-ChildItem -Path ( Join-Path $modulesDirDst -ChildPath "\Sitecore Speak 2*.zip" ) | %{$_.Name}
#
#	Write-Host "Installing module: " $moduleToInstall -ForegroundColor Green ; 
#	$urlInstallModules = $urlBase + "/InstallModules.aspx?modules=" + $moduleToInstall
#	Write-Host $urlInstallModules
#	Invoke-RestMethod $urlInstallModules -TimeoutSec 1200 
#
#	$moduleToInstall = Get-ChildItem -Path ( Join-Path $modulesDirDst -ChildPath "\Sitecore Speak Components 2*.zip" ) | %{$_.Name}
#
#	Write-Host "Installing module: " $moduleToInstall -ForegroundColor Green ; 
#	$urlInstallModules = $urlBase + "/InstallModules.aspx?modules=" + $moduleToInstall
#	Write-Host $urlInstallModules
#	Invoke-RestMethod $urlInstallModules -TimeoutSec 1200 
#}

Write-Host "Installing modules complete..." -ForegroundColor Green ; 
