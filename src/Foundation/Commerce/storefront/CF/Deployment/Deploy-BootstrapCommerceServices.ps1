#Requires -Version 3
. .\Deploy-Env-Common.ps1

# Bootstrap CommerceServices
Write-Host "BootStrapping Commerce Services" -ForegroundColor Yellow ; 
Invoke-RestMethod $urlCommerceShopsServicesBootstrap -TimeoutSec 1200 -Method PUT
Write-Host "Commerce Services BootStrapping completed" -ForegroundColor Green ; 