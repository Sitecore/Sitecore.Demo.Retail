Write-Host "Cleaning MongoDB Analytics" -ForegroundColor Green;
Write-Host "========================================" -ForegroundColor Green;
Write-Host "Dropping DB: analytics" -ForegroundColor White;
Write-Host "Dropping DB: tracking_live" -ForegroundColor White;
Write-Host "Dropping DB: tracking_history" -ForegroundColor White;

$DEPLOYMENT_DIRECTORY=(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent);
. ( Join-Path -Path $DEPLOYMENT_DIRECTORY -ChildPath "\BaseFunctions.ps1");

LoadEnvironmentXml -ConfigurationIdentity "Domain.Dev.SC";

# Find all mongo.exe and use the full path of the last exe found
$exes = get-childitem $($MONGO_EXE_PATH) -recurse | where {$_.Name -eq "mongo.exe"}
$id = $exes.count - 1
$MONGO_EXE = $exes[$id].FullName

# Drop the DBs
$cmd = "db.getSiblingDB('" + $Sitecore_MONGODB_Analytics_Name +"').dropDatabase();db.getSiblingDB('" + $Sitecore_MONGODB_Live_Name +"').dropDatabase();db.getSiblingDB('" + $Sitecore_MONGODB_History_Name +"').dropDatabase();"
. $MONGO_EXE mongodb --eval $cmd
