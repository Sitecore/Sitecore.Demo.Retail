#Requires -Version 3
. .\Deploy-Env-Common.ps1

# Import OpsApi Environments
Write-Host "Importing Commerce OpsApi Services environment..." -ForegroundColor Yellow ; 

$pathToJson  = ".\Environments\AdventureWorksOpsApi.json"
$json = Get-Content $pathToJson

Invoke-RestMethod $urlCommerceOpsApiServicesImportEnvironment -TimeoutSec 1200 -Method PUT -Body $json -ContentType 'application/json'
Write-Host "Commerce OpsApi Services environment import complete..." -ForegroundColor Green ; 

# Import Shops Environments
Write-Host "Importing Commerce Shops Services environment..." -ForegroundColor Yellow ; 

$pathToJson  = ".\Environments\AdventureWorksShops.json"
$json = Get-Content $pathToJson

Invoke-RestMethod $urlCommerceShopsServicesImportEnvironment -TimeoutSec 1200 -Method PUT -Body $json -ContentType 'application/json'
Write-Host "Commerce Shops Services environment import complete..." -ForegroundColor Green ; 

# Import Authoring Environments
Write-Host "Importing Commerce Authoring Services environment..." -ForegroundColor Yellow ; 

$pathToJson  = ".\Environments\AdventureWorksAuthoring.json"
$json = Get-Content $pathToJson

Invoke-RestMethod $urlCommerceAuthoringServicesImportEnvironment -TimeoutSec 1200 -Method PUT -Body $json -ContentType 'application/json'
Write-Host "Commerce Authoring Services environment import complete..." -ForegroundColor Green ; 

# Import Minions Environments
Write-Host "Importing Commerce Minions Services environment..." -ForegroundColor Yellow ; 

$pathToJson  = ".\Environments\AdventureWorksMinions.json"
$json = Get-Content $pathToJson

Invoke-RestMethod $urlCommerceMinionsServicesImportEnvironment -TimeoutSec 1200 -Method PUT -Body $json  -ContentType 'application/json'
Write-Host "Commerce Minions Services environment import complete..." -ForegroundColor Green ; 
