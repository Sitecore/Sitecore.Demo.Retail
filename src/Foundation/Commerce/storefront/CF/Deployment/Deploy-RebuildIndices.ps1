#Requires -Version 3
. .\Deploy-Env-Common.ps1

# Rebuild Index
Write-Host "Rebuilding index..." -ForegroundColor Green ; 
$urlRebuildIndex = $urlBase + "/RebuildIndex.aspx"
Invoke-RestMethod $urlRebuildIndex -TimeoutSec 2000
Write-Host "Rebuilding index complete..." -ForegroundColor Green ; 