#Requires -Version 3
. .\Deploy-Env-Common.ps1

# Bootstrap CommerceServices
#Write-Host "Cleaning  Global CommerceServices enviroment" -ForegroundColor Yellow ; 
# Clean global environment
#Invoke-RestMethod $urlCommerceServicesClean -TimeoutSec 1200 -Method PUT

# Clean AdventureWorks environment
Write-Host "Cleaning  CommerceShops environment" -ForegroundColor Yellow ; 
$headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
$headers.Add("Environment", 'AdventureWorksShops')
Invoke-RestMethod -Method POST -Uri $urlCommerceShopsServicesClean -Headers $headers -ContentType "application/json" -Body "{'environment':'AdventureWorksShops'}"
Write-Host "CommerceServices environment cleaning completed" -ForegroundColor Green ; 

Write-Host "Cleaning  CommerceAuthoring environment" -ForegroundColor Yellow ; 
$headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
$headers.Add("Environment", 'AdventureWorksAuthoring')
Invoke-RestMethod -Uri $urlCommerceAuthoringServicesClean -Headers $headers -ContentType "application/json" -Method POST -Body "{'environment':'AdventureWorksAuthoring'}"
Write-Host "CommerceServices environment cleaning completed" -ForegroundColor Green ; 

Write-Host "Cleaning  CommerceMinions environment" -ForegroundColor Yellow ; 
$headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
$headers.Add("Environment", 'AdventureWorksMinions')
Invoke-RestMethod -Uri $urlCommerceMinionsServicesClean -Headers $headers -ContentType "application/json" -Method POST -Body "{'environment':'AdventureWorksMinions'}"
Write-Host "CommerceServices environment cleaning completed" -ForegroundColor Green ;

Write-Host "Cleaning  CommerceOps Environment" -ForegroundColor Yellow ; 
$headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
$headers.Add("Environment", 'CommerceOps')
Invoke-RestMethod -Uri $urlCommerceOpsApiServicesClean -Headers $headers -ContentType "application/json" -Method POST -Body "{'environment':'CommerceOps'}"
Write-Host "CommerceOps cleaning completed" -ForegroundColor Green ;  