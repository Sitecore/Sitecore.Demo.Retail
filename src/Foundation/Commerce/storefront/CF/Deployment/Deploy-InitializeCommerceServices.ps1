#Requires -Version 3
. .\Deploy-Env-Common.ps1

# Initialize CommerceOps
Write-Host "Initializing CommerceOps..." -ForegroundColor Yellow ; 
Invoke-RestMethod $urlCommerceOpsApiServicesInitializeEnvironment -TimeoutSec 1200 -Method PUT
Write-Host "CommerceOps initialization complete..." -ForegroundColor Green ; 

# Initialize AdventureWorksShops
Write-Host "Initializing AdventureWorksShops..." -ForegroundColor Yellow ; 
Invoke-RestMethod $urlCommerceShopsServicesInitializeEnvironment -TimeoutSec 1200 -Method PUT
Write-Host "AdventureWorksShops initialization complete..." -ForegroundColor Green ; 

# Initialize AdventureWorksAuthoring
Write-Host "Initializing AdventureWorksAuthoring..." -ForegroundColor Yellow ; 
Invoke-RestMethod $urlCommerceAuthoringServicesInitializeEnvironment -TimeoutSec 1200 -Method PUT
Write-Host "AdventureWorksAuthoring initialization complete..." -ForegroundColor Green ; 

# Initialize AdventureWorksMinions
Write-Host "Initializing AdventureWorksMinions..." -ForegroundColor Yellow ; 
Invoke-RestMethod $urlCommerceMinionsServicesInitializeEnvironment -TimeoutSec 1200 -Method PUT
Write-Host "AdventureWorksMinions initialization complete..." -ForegroundColor Green ; 

# Initialize AdventureWorks (legacy AdventureWorks environment)
Write-Host "Initializing AdventureWorks (legacy)..." -ForegroundColor Yellow ; 
Invoke-RestMethod $urlAdventureWorksInitializeEnvironment -TimeoutSec 1200 -Method PUT
Write-Host "AdventureWorks (legacy) initialization complete..." -ForegroundColor Green ; 
