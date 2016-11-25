#Requires -Version 3
. .\Deploy-Env-Common.ps1

# Publish to Web
Write-Host "Publishing to web..." -ForegroundColor Green ; 
$urlPublish = $urlBase + "/Publish.aspx"
Invoke-RestMethod $urlPublish -TimeoutSec 1200
Write-Host "Publishing to web complete..." -ForegroundColor Green ; 