#Full Clean
. .\Deploy-Env-Common.ps1

trap
{
	Write-Host "Error: $($_.Exception.GetType().FullName)" -ForegroundColor Red ; 
	Write-Host $_.Exception.Message; 
	Write-Host $_.Exception.StackTrack;
	return;
}

function remove-dir([string]$dirname)
{
    try
    {
	    if(Test-Path $dirname)
        {
            Write-Host "Attemping to delete $dirname"
            Remove-Item $dirname -Recurse -Force
            Write-Host "$dirname Dir deleted" -ForegroundColor Green
        }
        else
        {
            Write-Warning "$dirname does not exist, no need to delete"
        }
    }
    catch
    {
	    Write-Error "Failed to delete $dirname"
    }
}

function Drop-SQL-Database-Delete
{
	param
	(
		[String]$dbName=$(throw 'Parameter -dbName is missing!')
	)
	
	try
	{
		$server = new-object ("Microsoft.SqlServer.Management.Smo.Server")
        if($server.Databases.Contains($dbName))
        {
            Write-Host "Attemping to delete database $dbName" -ForegroundColor Green -NoNewline
		    #Invoke-Sqlcmd -Query "ALTER DATABASE [$($dbName)] SET OFFLINE WITH ROLLBACK IMMEDIATE"
		    Invoke-Sqlcmd -Query "DROP DATABASE [$($dbName)]"
		    Write-Host "    DELETED" -ForegroundColor DarkGreen
        }
        else
        {
            Write-Warning "$dbName does not exist, cannot delete"
        }
	}
	catch
	{
		Write-Host "    Unable to delete database $dbName" -ForegroundColor Red
	}
}

function Drop-SQL-Login
{
	param
	(
		[String]$loginName=$(throw 'Parameter -loginName is missing!')
	)
	
	try
	{
		$server = new-object ("Microsoft.SqlServer.Management.Smo.Server")
        if($server.Logins.Contains($loginName))
        {
            Write-Host "Attemping to delete login $loginName" -ForegroundColor Green -NoNewline
		    Invoke-Sqlcmd -Query "DROP LOGIN [$($loginName)]"
		    Write-Host "    DELETED" -ForegroundColor DarkGreen
        }
        else
        {
            Write-Warning "$loginName does not exist, cannot delete"
        }
	}
	catch
	{
		Write-Host "    Unable to delete login $loginName" -ForegroundColor Red
	}
}


import-module webadministration

$DEPLOYMENT_DIRECTORY=(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent);

. ( Join-Path -Path $DEPLOYMENT_DIRECTORY -ChildPath "UtilityScripts\SQL.ps1");

Add-SQLPSSnapin


$PROJECT_NAME="CFSolutionStorefrontSite"
$ScriptPath = Split-Path $MyInvocation.InvocationName
$CSServicesSite = "CFCSServices"
$CommerceServicesSite = "CommerceServices"
$CSServicesDir = "C:\inetpub\CFCSServices"
$CommerceServicesDir = "C:\inetpub\CommerceServices"
$CSInstallDir = "C:\Program Files (x86)\Commerce Server 11"
[array]$CS_DBs = "MSCS_Admin","MSCS_CatalogScratch","$COMMERCESERVICES_DATABASE_NAME", "$COMMERCESERVICES_GLOBAL_DATABASE_NAME"
$MACHINE_NAME = gc env:computername
[array]$CS_Logins = "$MACHINE_NAME\CSCatalogUser","$MACHINE_NAME\CSFndRuntimeUser","$MACHINE_NAME\CSMarketingUser","$MACHINE_NAME\CSOrdersUser","$MACHINE_NAME\CSProfilesUser" 

# Clean Sitecore Install
Write-Host "Call Sitecore cleanup script" -ForegroundColor Green
& ".\CleanUpSitecoreInstall.ps1"

Sleep -Milliseconds 5000

#delete CS Site
Write-Host "Deleting the Commerce Site: $PROJECT_NAME" -ForegroundColor Green
Write-Host "Script Path: $ScriptPath" -ForegroundColor Green
$returnValue = Start-Process -FilePath  ".\tools\deletesite.exe" -ArgumentList @("$($PROJECT_NAME)") -Wait -PassThru;

if( $returnValue.ExitCode -ne 0 )
{
	Write-Host "Program cleaning up existing site." -ForegroundColor Red;
	Write-Error "Program Exit Code was $($returnValue.ExitCode), aborting.`r$($returnValue.StandardError)"
}

#uninstall CS
Write-Host "Uninstalling Commerce Server" -ForegroundColor Green
$exe = get-childitem -Path "C:\ProgramData\Package Cache" -Filter "CommerceServer*.exe" -Recurse
if($exe)
{
    Start-Process -FilePath $exe.FullName -ArgumentList '/uninstall','/silent' -Wait
}

#delete cs folder
Write-Host "Remove the Commerce Server install folder" -ForegroundColor Green
remove-dir $CSInstallDir

#Delete DB's
Write-Host "Attemping to delete left over db's" -ForegroundColor Green
foreach($db in $CS_DBs)
{
    Drop-SQL-Database-Delete $db
}

#Delete DB users
Write-Host "Attemping to delete sql logins" -ForegroundColor Green
foreach($login in $CS_Logins)
{
    Drop-SQL-Login $login
}

#Delete Commerce DB
Write-Host "Attemping to delete Commerce DB" -ForegroundColor Green
Write-Host "Deleting $commerceServicesDbName" -ForegroundColor Green
Drop-SQL-Database-Delete "$commerceServicesDbName"
Write-Host "Deleting $commerceServicesGlobalDbName" -ForegroundColor Green
Drop-SQL-Database-Delete "$commerceServicesGlobalDbName"

#delete CSServices site
if(Test-Path IIS:\Sites\$CSServicesSite)
{
    Write-Host "Removing website $CSServicesSite"
    Invoke-Expression -Command "& $($env:systemroot)\system32\inetsrv\APPCMD.exe delete site `"$CSServicesSite`"";
    $stopIIS = $true
}
else
{
    Write-Warning "Website $CSServicesSite does not exist, no need to delete"
}

#delete CommerceServices site
if(Test-Path IIS:\Sites\$CommerceServicesSite)
{
    Write-Host "Removing website $CommerceServicesSite"
    Invoke-Expression -Command "& $($env:systemroot)\system32\inetsrv\APPCMD.exe delete site `"$CommerceServicesSite`"";
    $stopIIS = $true
}
else
{
    Write-Warning "Website $CommerceServicesSite does not exist, no need to delete"
}

#remove CSServices folder
Write-Host "Deleting the Commerce Server Service dir" -ForegroundColor Green
remove-dir $CSServicesDir

#remove CommerceServices folder
Write-Host "Deleting the Commerce Services dir" -ForegroundColor Green
remove-dir $CommerceServicesDir

#delete IIS users and AppPools
#TODO

#Delete Windows users and groups
#TODO

#iisreset
iisreset

#END

