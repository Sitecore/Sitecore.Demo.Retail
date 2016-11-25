#Requires -Version 3
. .\Deploy-Env-Common.ps1

# Deploy EA Plans
Write-Host "Deploying EA Plans..." -ForegroundColor Green ; 
#$nunitExe = "C:\Program Files (x86)\NUnit 2.6.3\bin\nunit-console.exe"
$testBin = $installDir + "\Website\bin\Sitecore.Commerce.Tests.QA.dll"
$test1 = "Sitecore.Obec.Tests.QA.TestEnvironmentInitialization.Step01ShouldCreateAbandonedCartEngagementPlan"
$test2 = "Sitecore.Obec.Tests.QA.TestEnvironmentInitialization.Step02ShouldDeployAbandonedCartEngagementPlan"
$test3 = "Sitecore.Obec.Tests.QA.TestEnvironmentInitialization.Step03ShouldCreateNewOrderPlacedCartEngagementPlan"
$test4 = "Sitecore.Obec.Tests.QA.TestEnvironmentInitialization.Step04ShouldDeployNewOrderPlacedEngagementPlan"
& "C:\Program Files (x86)\NUnit 2.6.3\bin\nunit-console.exe" /run:$test1 $testBin
& "C:\Program Files (x86)\NUnit 2.6.3\bin\nunit-console.exe" /run:$test2 $testBin
& "C:\Program Files (x86)\NUnit 2.6.3\bin\nunit-console.exe" /run:$test3 $testBin
& "C:\Program Files (x86)\NUnit 2.6.3\bin\nunit-console.exe" /run:$test4 $testBin
Write-Host "Deploying EA Plans complete..." -ForegroundColor Green ; 