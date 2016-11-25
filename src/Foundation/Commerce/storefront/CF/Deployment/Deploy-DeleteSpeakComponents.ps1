#Requires -Version 3
. .\Deploy-Env-Common.ps1

# Publish to Web
Write-Host "Deleting Speak Components..." -ForegroundColor Green ; 
$urlPublish = $urlBase + "/DeleteSpeakComponentsItems.aspx"
Invoke-RestMethod $urlPublish -TimeoutSec 1200
Write-Host "Deleting Speak components complete..." -ForegroundColor Green ; 