#Requires -Version 3
. ( Join-Path -Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent) -ChildPath "Deploy.ps1" );

# make sure no files are locked
stop-service W3SVC
start-service W3SVC

# do the merge
Deploy-CommerceServer -ConfigurationIdentity "Domain.Dev.Merge" -PreCompile;

# done
Write-Host "Merging files for Sitecore Instance $instanceName is complete" -ForegroundColor Green
