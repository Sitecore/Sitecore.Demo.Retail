#Requires -Version 3
. .\Deploy-Env-Common.ps1

IF ($deployConfig -match "DevDeploy")
{
	& nuget restore .\packages.config -PackagesDirectory .\packages -Config ..\..\.nuget\NuGet.Config 
}

 .\CleanUpSitecoreInstall.ps1
 .\Deploy-SC-CS.ps1
