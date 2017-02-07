#Requires -Version 3
. ( Join-Path -Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent) -ChildPath "Deploy.ps1" );

Deploy-CommerceServer -ConfigurationIdentity "Domain.Dev.Existing" -PreCompile;

# done
Write-Host "Done setting up Sitecore Instance $instanceName" -ForegroundColor Green
