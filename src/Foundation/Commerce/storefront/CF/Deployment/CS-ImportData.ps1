#Requires -Version 3

param (
		[string]$csSiteName = "CSSolutionStorefrontsite", 
		[string]$catalogFile,
		[string]$inventoryFile,
		[string]$marketingFile,
		[string]$ordersFile,
		[string]$ordersConfigFile,
		[string]$profilesFile,
		[string]$profilesDacpacFile,
		[string]$catalogAzmanFile
)

if ((Get-Module -ListAvailable CSPS) -eq $null)
{
    Write-Host "Importing CSPS Module using direct path"
    Import-Module "C:\Program Files (x86)\Commerce Server 11\PowerShell\Modules\CSPS\CSPS.psd1"
}
else
{
    Write-Host "Importing CSPS Module"
    Import-Module CSPS
}

function Convert-ToProperties
(
	[parameter(Mandatory = $true)]
	$propertyString
)
{
    $propertyString.Split(';') | % {
      $key, $value = $_.split('=');
      if($key)
      {
        $propertyString = $propertyString | Add-Member -PassThru Noteproperty ($key -Replace " ", "").ToLower() $value
      }
    }
	
    return $propertyString;
}

function DeployDacpac
(
	[Parameter(Mandatory=$true)][string]$dacpac, 
	[Parameter(Mandatory=$true)]$connStr,
	[bool]$deleteBeforeCreate = $FALSE	
)
{
	Write-Host "Importing DACPAC $($dacpac)"
    $connStr = $connStr.replace("Provider=SQLOLEDB;", "");
	$connStr = Convert-ToProperties $connStr;
	$databaseName = $connStr.initialcatalog;

	# load in DAC DLL (requires config file to support .NET 4.0)
	# change file location for a 32-bit OS
	# param out the base path of SQL Server
	$sqlServerVersions = @("120", "110");
	$baseSQLServerPath = "C:\Program Files (x86)\Microsoft SQL Server\{0}\DAC\bin\Microsoft.SqlServer.Dac.dll";

	foreach($sqlServerVersion in $sqlServerVersions)
	{
		$fullPath = $baseSQLServerPath -f $sqlServerVersion;

		if(Test-Path -Path $fullPath)
		{
			Write-Host "Using SQL Server $($sqlServerVersion) to import DACPAC";
			add-type -path $fullPath;

			break;
		}
	}

	# make DacServices object, needs a connection string
	$d = new-object Microsoft.SqlServer.Dac.DacServices $connStr;

	# register events, if you want 'em
	register-objectevent -in $d -eventname Message -source "msg" -action { out-host -in $Event.SourceArgs[1].Message.Message }

	# Load dacpac from file & deploy to database named pubsnew
	 $dp = [Microsoft.SqlServer.Dac.DacPackage]::Load($dacpac)
	 $d.deploy($dp, $databaseName, $TRUE);

	# clean up event
	unregister-event -source "msg";
}

if($catalogFile) { Import-CSCatalog -Name $csSiteName -File $catalogFile -AzMan $catalogAzmanFile; }
if($inventoryFile) { Import-CSInventory -Name $csSiteName -File $inventoryFile -AzMan $catalogAzmanFile; }
if($marketingFile) { Import-CSMarketing -Name $csSiteName -File $marketingFile; }
if($ordersFile) { Import-CSOrders -Name $csSiteName -File $ordersFile; }
if($ordersConfigFile) { Import-CSOrdersConfig -Name $csSiteName -File $ordersConfigFile; }
if($profilesFile) { Import-CSProfiles -Name $csSiteName -File $profilesFile; }


# Run DACPACS/BACPACKS
$profileGlobal = Get-CSSiteResourceProperty -Name $csSiteName -Resource "Biz Data Service" -PropertyName s_RefResource;
$profileConnStr = Get-CSGlobalResourceProperty -Resource $profileGlobal -PropertyName "s_BizDataStoreConnectionString";

if($profilesDacpacFile){ DeployDacpac $profilesDacpacFile $profileConnStr $true };