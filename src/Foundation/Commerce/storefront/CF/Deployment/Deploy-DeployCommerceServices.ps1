#Requires -Version 3
. .\BaseFunctions.ps1
. .\Deploy-Env-Common.ps1

$configuration = LoadEnvironmentXml -ConfigurationIdentity "Domain.Dev.Unpup"; 

. ( Join-Path -Path $DEPLOYMENT_DIRECTORY -ChildPath "UtilityScripts\zip.ps1");
. ( Join-Path -Path $DEPLOYMENT_DIRECTORY -ChildPath "UtilityScripts\Windows.ps1");
. ( Join-Path -Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent) -ChildPath "Deploy.ps1" );

#************************************************************************
#**** CLEANUP PREVIOUS INSTALL ****
#************************************************************************

#Remove the Commerce OpsApi Services web site if it exists
if(Get-Website("$commerceOpsApiServicesSiteName"))
{
    Write-Host "Removing Commerce OpsApi Services web site"
    Stop-Website -Name $commerceOpsApiServicesSiteName
    Remove-Website -Name $commerceOpsApiServicesSiteName
}

#Remove the Commerce Shops Services web site if it exists
if(Get-Website("$commerceShopsServicesSiteName"))
{
    Write-Host "Removing Commerce Shops Services web site"
    Stop-Website -Name $commerceShopsServicesSiteName
    Remove-Website -Name $commerceShopsServicesSiteName
}

#Remove the Commerce Authoring Services web site if it exists
if(Get-Website("$commerceAuthoringServicesSiteName"))
{
    Write-Host "Removing Commerce Authoring Services web site"
    Stop-Website -Name $commerceAuthoringServicesSiteName
    Remove-Website -Name $commerceAuthoringServicesSiteName
}

#Remove the Commerce Minions Services web site if it exists
if(Get-Website("$commerceMinionsServicesSiteName"))
{
    Write-Host "Removing Commerce Minions Services web site"
    Stop-Website -Name $commerceMinionsServicesSiteName
    Remove-Website -Name $commerceMinionsServicesSiteName
}

#Remove the Commerce OpsApi Services application pool if it exists
if(Test-Path "IIS:\AppPools\$commerceOpsApiServicesAppPoolName")
{
    if((Get-WebAppPoolState $commerceOpsApiServicesAppPoolName).Value -eq "Started")
    {
        Write-Host "Stopping Commerce OpsApi Services application pool"
        Stop-WebAppPool -Name $commerceOpsApiServicesAppPoolName
    }
    Write-Host "Removing Commerce OpsApi Services application pool"
    Remove-WebAppPool -Name $commerceOpsApiServicesAppPoolName
}

#Remove the Commerce Shops Services application pool if it exists
if(Test-Path "IIS:\AppPools\$commerceShopsServicesAppPoolName")
{
    if((Get-WebAppPoolState $commerceShopsServicesAppPoolName).Value -eq "Started")
    {
        Write-Host "Stopping Commerce Shops Services application pool"
        Stop-WebAppPool -Name $commerceShopsServicesAppPoolName
    }
    Write-Host "Removing Commerce Shops Services application pool"
    Remove-WebAppPool -Name $commerceShopsServicesAppPoolName
}

#Remove the Commerce Authoring Services application pool if it exists
if(Test-Path "IIS:\AppPools\$commerceAuthoringServicesAppPoolName")
{
    if((Get-WebAppPoolState $commerceAuthoringServicesAppPoolName).Value -eq "Started")
    {
        Write-Host "Stopping Commerce Authoring Services application pool"
        Stop-WebAppPool -Name $commerceAuthoringServicesAppPoolName
    }
    Write-Host "Removing Commerce Authoring Services application pool"
    Remove-WebAppPool -Name $commerceAuthoringServicesAppPoolName
}

#Remove the Commerce Minions Services application pool if it exists
if(Test-Path "IIS:\AppPools\$commerceMinionsServicesAppPoolName")
{
    if((Get-WebAppPoolState $commerceMinionsServicesAppPoolName).Value -eq "Started")
    {
        Write-Host "Stopping Commerce Minions Services application pool"
        Stop-WebAppPool -Name $commerceMinionsServicesAppPoolName
    }
    Write-Host "Removing Commerce Minions Services application pool"
    Remove-WebAppPool -Name $commerceMinionsServicesAppPoolName
}

# DELETE COMMERCE OPSAPI SERVICES SERVICE FOLDER
try
{
	if(Test-Path $COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY)
    {
        Write-Host "Attempting to delete site directory $COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY"
        Remove-Item $COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY -Recurse -Force
        Write-Host "$COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY deleted" -ForegroundColor Green
    }
    else
    {
        Write-Warning "$COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY does not exist, no need to delete"
    }
}
catch
{
	Write-Error "Failed to delete $COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY"
}

# DELETE COMMERCE SHOPS SERVICES SERVICE FOLDER
try
{
	if(Test-Path $COMMERCE_SHOPS_SERVICES_WS_DIRECTORY)
    {
        Write-Host "Attempting to delete site directory $COMMERCE_SHOPS_SERVICES_WS_DIRECTORY"
        Remove-Item $COMMERCE_SHOPS_SERVICES_WS_DIRECTORY -Recurse -Force
        Write-Host "$COMMERCE_SHOPS_SERVICES_WS_DIRECTORY deleted" -ForegroundColor Green
    }
    else
    {
        Write-Warning "$COMMERCE_SHOPS_SERVICES_WS_DIRECTORY does not exist, no need to delete"
    }
}
catch
{
	Write-Error "Failed to delete $COMMERCE_SHOPS_SERVICES_WS_DIRECTORY"
}

# DELETE COMMERCE AUTHORING SERVICES SERVICE FOLDER
try
{
	if(Test-Path $COMMERCE_AUTHORING_SERVICES_WS_DIRECTORY)
    {
        Write-Host "Attempting to delete site directory $COMMERCE_AUTHORING_SERVICES_WS_DIRECTORY"
        Remove-Item $COMMERCE_AUTHORING_SERVICES_WS_DIRECTORY -Recurse -Force
        Write-Host "$COMMERCE_AUTHORING_SERVICES_WS_DIRECTORY deleted" -ForegroundColor Green
    }
    else
    {
        Write-Warning "$COMMERCE_AUTHORING_SERVICES_WS_DIRECTORY does not exist, no need to delete"
    }
}
catch
{
	Write-Error "Failed to delete $COMMERCE_AUTHORING_SERVICES_WS_DIRECTORY"
}

# DELETE COMMERCE MINIONS SERVICES SERVICE FOLDER
try
{
	if(Test-Path $COMMERCE_MINIONS_SERVICES_WS_DIRECTORY)
    {
        Write-Host "Attempting to delete site directory $COMMERCE_MINIONS_SERVICES_WS_DIRECTORY"
        Remove-Item $COMMERCE_MINIONS_SERVICES_WS_DIRECTORY -Recurse -Force
        Write-Host "$COMMERCE_MINIONS_SERVICES_WS_DIRECTORY deleted" -ForegroundColor Green
    }
    else
    {
        Write-Warning "$COMMERCE_MINIONS_SERVICES_WS_DIRECTORY does not exist, no need to delete"
    }
}
catch
{
	Write-Error "Failed to delete $COMMERCE_MINIONS_SERVICES_WS_DIRECTORY"
}

# Deploy CommerceServices
$CommerceServicesZip = $( Join-Path -Path $COMMERCESERVICES_DEPENDENCIES_DIRECTORY -ChildPath "Zips\Sitecore.Commerce.Engine.*" )
$CommerceServicesSQL = $( Join-Path -Path $COMMERCESERVICES_DEPENDENCIES_DIRECTORY -ChildPath "Zips\CommerceServicesDbScript.sql" )
IF ($deployConfig -match "DevDeploy")
{
	$CommerceServicesZip = Resolve-Path -Path ".\packages\Sitecore.Commerce.Engine.1*\content\Sitecore.Commerce.Engine.*"
	$CommerceServicesSQL = Resolve-Path -Path ".\packages\Sitecore.Commerce.Engine.1*\content\CommerceServicesDbScript.sql"
}

#Drop and re-create the CommerceServices database
Write-Host "Creating CommerceServices database...";
Add-SQLPSSnapin;

$SQL_VARIABLES_BASE=@("CS_USER_FOUNDATION=`"$(( $configuration.UserAccounts.UserAccount | where { $_.identity -ieq "CSFoundationUser" }).domain )\$(( $configuration.UserAccounts.UserAccount | where { $_.identity -ieq "CSFoundationUser" }).userName )`"");			
$databaseServer =  "$($Env:COMPUTERNAME)";
$SQL_VARIABLES = $SQL_VARIABLES_BASE;
$SQL_VARIABLES += "CS_DB_SERVER=`"$($databaseServer)`"";
	
$SQL_VARIABLES_STRING = [System.String]::Join( " -v ", $SQL_VARIABLES );
$returnValue = Start-Process -Wait -NoNewWindow -PassThru -WorkingDirectory $DEPLOYMENT_DIRECTORY -FilePath $SQLCMD_PATH -ArgumentList @( "-b", "-E", "-S $($databaseServer)", "-i `"$($CommerceServicesSQL)`"", "-v $($SQL_VARIABLES_STRING)");
Write-Host $returnValue.StandardOutput;
if( $returnValue.ExitCode -ne 0 )
{
	if( $Error )
	{
		Write-Host $Error -ForegroundColor Red;
	}
	Write-Error -Message $returnValue.StandardError -ErrorId $returnValue.ExitCode ;
}

#************************************************************************
#***** SERVICE EXTRACTION AND COPY ****
#************************************************************************

#**** OPS API ****

Write-Host
#Extracting the CommerceServices zip file Commerce Shops Services
Write-Host "Extracting CommerceServices from $CommerceServicesZip to $COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY" -ForegroundColor Yellow ; 
UnZipFile $CommerceServicesZip $COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY
Write-Host "Commerce OpsApi Services extraction completed" -ForegroundColor Green ; 

$CommerceServicesLogDir = $(Join-Path -Path $COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY -ChildPath "wwwroot\logs")

Write-Host "Creating Commerce Shops Services logs directory at: $CommerceServicesLogDir"
New-Item -Path $COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY -Name "wwwroot\logs" -ItemType "directory"

Write-Host "Deployment Config Type: $DEPLOY_CONFIG";

Write-Host "Granting full access to '$($DOMAIN_NAME)\CSFndRuntimeUser' to logs directory: $CommerceServicesLogDir"
Windows-GrantFullReadWriteAccessToFile -Path $CommerceServicesLogDir  -UserName "$($DOMAIN_NAME)\CSFndRuntimeUser"

# Set the proper environment name
$pathToJson  = $(Join-Path -Path $COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY -ChildPath "wwwroot\config.json")
$originalJson = Get-Content $pathToJson -Raw  | ConvertFrom-Json
$originalJson.AppSettings.EnvironmentName = "AdventureWorksOpsApi"
$originalJson | ConvertTo-Json -Compress | set-content $pathToJson

#**** SHOPS ****

# Copy the the CommerceServices files to the Commerce Shops Services
Write-Host "Copying Commerce Services from $COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY to $COMMERCE_SHOPS_SERVICES_WS_DIRECTORY" -ForegroundColor Yellow ; 
Copy-Item -Path $COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY -Destination $COMMERCE_SHOPS_SERVICES_WS_DIRECTORY -Force -Recurse
Write-Host "Commerce Shops Services extraction completed" -ForegroundColor Green ; 

$CommerceServicesLogDir = $(Join-Path -Path $COMMERCE_SHOPS_SERVICES_WS_DIRECTORY -ChildPath "wwwroot\logs")

Write-Host "Deployment Config Type: $DEPLOY_CONFIG";

Write-Host "Granting full access to '$($DOMAIN_NAME)\CSFndRuntimeUser' to logs directory: $CommerceServicesLogDir"
Windows-GrantFullReadWriteAccessToFile -Path $CommerceServicesLogDir  -UserName "$($DOMAIN_NAME)\CSFndRuntimeUser"

# Set the proper environment name
$pathToJson  = $(Join-Path -Path $COMMERCE_SHOPS_SERVICES_WS_DIRECTORY -ChildPath "wwwroot\config.json")
$originalJson = Get-Content $pathToJson -Raw | ConvertFrom-Json
$originalJson.AppSettings.EnvironmentName = "AdventureWorksShops"
$originalJson | ConvertTo-Json -Compress | set-content $pathToJson

#**** AUTHORING ****

# Copy the the CommerceServices files to the Commerce Authoring Services
Write-Host "Copying Commerce Services from $COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY to $COMMERCE_AUTHORING_SERVICES_WS_DIRECTORY" -ForegroundColor Yellow ; 
Copy-Item -Path $COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY -Destination $COMMERCE_AUTHORING_SERVICES_WS_DIRECTORY -Force -Recurse
Write-Host "Commerce Autoring Services extraction completed" -ForegroundColor Green ; 

$CommerceServicesLogDir = $(Join-Path -Path $COMMERCE_AUTHORING_SERVICES_WS_DIRECTORY -ChildPath "wwwroot\logs")
Write-Host "Deployment Config Type: $DEPLOY_CONFIG";

Write-Host "Granting full access to '$($DOMAIN_NAME)\CSFndRuntimeUser' to logs directory: $CommerceServicesLogDir"
Windows-GrantFullReadWriteAccessToFile -Path $CommerceServicesLogDir  -UserName "$($DOMAIN_NAME)\CSFndRuntimeUser"

# Set the proper environment name
$pathToJson  = $(Join-Path -Path $COMMERCE_AUTHORING_SERVICES_WS_DIRECTORY -ChildPath "wwwroot\config.json")
$originalJson = Get-Content $pathToJson -Raw | ConvertFrom-Json
$originalJson.AppSettings.EnvironmentName = "AdventureWorksAuthoring"
$originalJson | ConvertTo-Json -Compress | set-content $pathToJson

#**** MINIONS ****

# Copy the the CommerceServices files to the Commerce Minions Services
Write-Host "Copying Commerce Services from $COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY to $COMMERCE_MINIONS_SERVICES_WS_DIRECTORY" -ForegroundColor Yellow ; 
Copy-Item -Path $COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY -Destination $COMMERCE_MINIONS_SERVICES_WS_DIRECTORY -Force -Recurse
Write-Host "Commerce Minions Services extraction completed" -ForegroundColor Green ; 
$CommerceServicesLogDir = $(Join-Path -Path $COMMERCE_MINIONS_SERVICES_WS_DIRECTORY -ChildPath "wwwroot\logs")

Write-Host "Deployment Config Type: $DEPLOY_CONFIG";

Write-Host "Granting full access to '$($DOMAIN_NAME)\CSFndRuntimeUser' to logs directory: $CommerceServicesLogDir"
Windows-GrantFullReadWriteAccessToFile -Path $CommerceServicesLogDir  -UserName "$($DOMAIN_NAME)\CSFndRuntimeUser"

# Set the proper environment name
$pathToJson  = $(Join-Path -Path $COMMERCE_MINIONS_SERVICES_WS_DIRECTORY -ChildPath "wwwroot\config.json")
$originalJson = Get-Content $pathToJson -Raw | ConvertFrom-Json
$originalJson.AppSettings.EnvironmentName = "AdventureWorksMinions"
$originalJson | ConvertTo-Json -Compress | set-content $pathToJson


#************************************************************************
#**** APPLICATION POOLS CREATION ****
#************************************************************************

Write-Host

#Create the application pool for the OpsApi Services
Write-Host "Creating and starting the Commerce OpsApi Services application pool" -ForegroundColor Yellow
$appPoolInstance = New-WebAppPool -Name $commerceOpsApiServicesAppPoolName
$userAccountInstance = $configuration.UserAccounts.UserAccount | where { $_.identity -ieq "CSFoundationUser" };

if($userAccountInstance -ne $null)
{
    $appPoolInstance.processModel.identityType = 3;
    $appPoolInstance.processModel.userName = "$($userAccountInstance.domain)\$($userAccountInstance.username)";
    $appPoolInstance.processModel.password = $userAccountInstance.password;
    $appPoolInstance | Set-Item;
}

$appPoolInstance.managedPipelineMode = "Integrated";
$appPoolInstance.managedRuntimeVersion = "";	
$appPoolInstance | Set-Item		
Start-WebAppPool -Name $commerceOpsApiServicesAppPoolName
Write-Host "Creation of the Commerce OpsApi Services application pool completed" -ForegroundColor Green ; 

#Create the application pool for the Shops Services (with 2 worker processes)
Write-Host "Creating and starting the Commerce Shops Services application pool (with 2 worker processes)" -ForegroundColor Yellow
$appPoolInstance = New-WebAppPool -Name $commerceShopsServicesAppPoolName
$userAccountInstance = $configuration.UserAccounts.UserAccount | where { $_.identity -ieq "CSFoundationUser" };

if($userAccountInstance -ne $null)
{
    $appPoolInstance.processModel.identityType = 3;
    $appPoolInstance.processModel.userName = "$($userAccountInstance.domain)\$($userAccountInstance.username)";
    $appPoolInstance.processModel.password = $userAccountInstance.password;
    $appPoolInstance.processModel.maxProcesses = 2;
    $appPoolInstance.processModel.loadUserProfile = 0;
    $appPoolInstance | Set-Item;
}

$appPoolInstance.managedPipelineMode = "Integrated";
$appPoolInstance.managedRuntimeVersion = "";
$appPoolInstance | Set-Item		
Start-WebAppPool -Name $commerceShopsServicesAppPoolName
Write-Host "Creation of the Commerce Shops Services application pool completed" -ForegroundColor Green ; 

#Create the application pool for the Authoring Services
Write-Host "Creating and starting the Commerce Authoring Services application pool" -ForegroundColor Yellow
$appPoolInstance = New-WebAppPool -Name $commerceAuthoringServicesAppPoolName
$userAccountInstance = $configuration.UserAccounts.UserAccount | where { $_.identity -ieq "CSFoundationUser" };

if($userAccountInstance -ne $null)
{
    $appPoolInstance.processModel.identityType = 3;
    $appPoolInstance.processModel.userName = "$($userAccountInstance.domain)\$($userAccountInstance.username)";
    $appPoolInstance.processModel.password = $userAccountInstance.password;
    $appPoolInstance | Set-Item;
}

$appPoolInstance.managedPipelineMode = "Integrated";
$appPoolInstance.managedRuntimeVersion = "";	
$appPoolInstance | Set-Item		
Start-WebAppPool -Name $commerceAuthoringServicesAppPoolName
Write-Host "Creation of the Commerce Authoring Services application pool completed" -ForegroundColor Green ; 

#Create the application pool for the Minions Services
Write-Host "Creating and starting the Commerce Minions Services application pool" -ForegroundColor Yellow
$appPoolInstance = New-WebAppPool -Name $commerceMinionsServicesAppPoolName
$userAccountInstance = $configuration.UserAccounts.UserAccount | where { $_.identity -ieq "CSFoundationUser" };

if($userAccountInstance -ne $null)
{
    $appPoolInstance.processModel.identityType = 3;
    $appPoolInstance.processModel.userName = "$($userAccountInstance.domain)\$($userAccountInstance.username)";
    $appPoolInstance.processModel.password = $userAccountInstance.password;
    $appPoolInstance | Set-Item;
}

$appPoolInstance.managedPipelineMode = "Integrated";
$appPoolInstance.managedRuntimeVersion = "";	
$appPoolInstance | Set-Item		
Start-WebAppPool -Name $commerceMinionsServicesAppPoolName
Write-Host "Creation of the Commerce Minions Services application pool completed" -ForegroundColor Green ; 

#************************************************************************
#**** WEB SERVICES SITE CREATION ****
#************************************************************************

Write-Host

#Create the OpsApi service web site
Write-Host "Creating and starting the Commerce OpsApi Services web site" -ForegroundColor Yellow
New-Website -Name $commerceOpsApiServicesSiteName -ApplicationPool $commerceOpsApiServicesAppPoolName -PhysicalPath $COMMERCE_OPSAPI_SERVICES_WS_DIRECTORY -Port $commerceOpsApiServicesPort
Start-Website -Name $commerceOpsApiServicesSiteName
Write-Host "Creation and startup of the Commerce OpsApi Services web site completed" -ForegroundColor Green ; 

Write-Host

#Create the shops service web site
Write-Host "Creating and starting the Commerce Shops Services web site" -ForegroundColor Yellow
New-Website -Name $commerceShopsServicesSiteName -ApplicationPool $commerceShopsServicesAppPoolName -PhysicalPath $COMMERCE_SHOPS_SERVICES_WS_DIRECTORY -Port $commerceShopsServicesPort
Start-Website -Name $commerceShopsServicesSiteName
Write-Host "Creation and startup of the Commerce Shops Services web site completed" -ForegroundColor Green ; 

Write-Host

#Create the authoring service web site
Write-Host "Creating and starting the Commerce Authoring Services web site" -ForegroundColor Yellow
New-Website -Name $commerceAuthoringServicesSiteName -ApplicationPool $commerceAuthoringServicesAppPoolName -PhysicalPath $COMMERCE_AUTHORING_SERVICES_WS_DIRECTORY -Port $commerceAuthoringServicesPort
Start-Website -Name $commerceAuthoringServicesSiteName
Write-Host "Creation and startup of the Commerce Authoring Services web site completed" -ForegroundColor Green ; 

Write-Host

#Create the MINIONS service web site
Write-Host "Creating and starting the Commerce Minions Services web site" -ForegroundColor Yellow
New-Website -Name $commerceMinionsServicesSiteName -ApplicationPool $commerceMinionsServicesAppPoolName -PhysicalPath $COMMERCE_MINIONS_SERVICES_WS_DIRECTORY -Port $commerceMinionsServicesPort
Start-Website -Name $commerceMinionsServicesSiteName
Write-Host "Creation and startup of the Commerce Minions Services web site completed" -ForegroundColor Green ; 