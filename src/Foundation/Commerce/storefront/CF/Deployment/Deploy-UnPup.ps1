#Requires -Version 3
. .\Deploy-Env-Common.ps1

IF ($deployConfig -match "DevDeploy")
{
	& nuget restore .\packages.config -PackagesDirectory .\packages -Config ..\..\.nuget\NuGet.Config 
}

. ( Join-Path -Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent) -ChildPath "Deploy.ps1" );

Deploy-CommerceServer -ConfigurationIdentity "Domain.Dev.Unpup" -PreCompile;