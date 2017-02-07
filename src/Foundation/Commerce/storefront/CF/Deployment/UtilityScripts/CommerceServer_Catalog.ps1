#Requires -Version 2
$ErrorActionPreference = "Stop";
Set-PSDebug -Strict;

################################################################
#.Synopsis
#  Import an exported Inventory Schema
#.Parameter CommerceSiteName
#  The Commerce Server site name
#.Parameter FilePath
#  The path to the exported xml file.
################################################################
function Import-CS2007InventorySchema
{
	PARAM
	(
	    [String]
		$CommerceSiteName=$(throw 'Parameter -CommerceSiteName is missing!'),

	    [String]
		$FilePath=$(throw 'Parameter -FilePath is missing!'),

		[parameter(Mandatory = $false)]
		[String]
		$AzManFile=$null
	)
	# If you do not do this, in some cases it returns a "Invalid database schema version" error
	Write-Host "Restart the Commerce Server Catalog Import Host COM+ application";
	Restart-CSCatalogImportHost -ComApplicationName "Commerce Server Catalog Import Host";

	$argumentListReg = @( $CommerceSiteName, """$FilePath""", """$AzManFile""" );

	Write-Host "Running file $IMPORT_INVENTORY"
	Write-Host "Importing inventory for $CommerceSiteName" -ForegroundColor Green;

	$returnValue = Start-Process -FilePath $IMPORT_INVENTORY -Wait -NoNewWindow -ArgumentList $argumentListReg -PassThru;

	Write-Host $returnValue.StandardOutput;

	if( $returnValue.ExitCode -ne 0 )
	{
		Write-Error -Message $returnValue.StandardError -ErrorId $returnValue.ExitCode ;
	}
}

################################################################
#.Synopsis
#  Import an exported Catalog Schema
#.Parameter CommerceSiteName
#  The Commerce Server site name
#.Parameter FilePath
#  The path to the exported xml file.
################################################################
function Import-CS2007CatalogSchema
{
	PARAM
	(
	    [String]$CommerceSiteName=$(throw 'Parameter -CommerceSiteName is missing!'),
	    [String]$FilePath=$(throw 'Parameter -FilePath is missing!'),
	    $Replace=$false,
	    $Transacted=$false,
	    $AllowSchemaUpdate=$false
	)

    Write-Host "Loading .NET Assemblies...";
	[void][System.Reflection.Assembly]::Load("CommerceServer.Core.CrossTier, Version=10.0.0.0, Culture=neutral, PublicKeyToken=f5c79cb11734af7a")
    [void][System.Reflection.Assembly]::Load("CommerceServer.Core.Catalog, Version=10.0.0.0, Culture=neutral, PublicKeyToken=f5c79cb11734af7a")
    [void][System.Reflection.Assembly]::Load("ADODB, Version=7.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
    [void][System.Reflection.Assembly]::Load("System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
    
	# If you do not do this, in some cases it returns a "Invalid database schema version" error
	Write-Host "Restart the Commerce Server Catalog Import Host COM+ application";
	net stop COMSysApp;
	net start COMSysApp;
	Restart-CSCatalogImportHost -ComApplicationName "Commerce Server Catalog Import Host";

	Write-Host "Creating a CatalogSiteAgent to connect to the database.";
  	$catalogSiteAgent = New-Object CommerceServer.Core.Catalog.CatalogSiteAgent;
    $catalogSiteAgent.SiteName = $CommerceSiteName;
    $catalogSiteAgent.AuthorizationMode = [CommerceServer.Core.AuthorizationMode]::NoAuthorization;
    $catalogSiteAgent.IgnoreInventorySystem = $false;

	$catalogContext = [CommerceServer.Core.Catalog.CatalogContext]::Create($catalogSiteAgent);

	Write-Host "Getting the Catalog Context";
    $catalogContext = $catalogContext.CatalogContext;

	Write-Host "ImportSchemaChanges: $($AllowSchemaUpdate)";
	Write-Host "OverwriteRelationships: $($Replace)";
	Write-Host "Transacted: $($Transacted)";
	
	$importOptions = New-Object CommerceServer.Core.Catalog.CatalogImportOptions;
    $importOptions.Mode = [CommerceServer.Core.Catalog.ImportMode]::Full;
	$importOptions.ImportSchemaChanges = $AllowSchemaUpdate;
	$importOptions.OverwriteRelationships = $Replace;
    $importOptions.Operation = [CommerceServer.Core.Catalog.ImportOperation]::Import;
    
    if( $Transacted )
    {
		$importOptions.TransactionMode = [CommerceServer.Core.Catalog.TransactionMode]::NonTransactional;
    }
    else
    {
		$importOptions.TransactionMode = [CommerceServer.Core.Catalog.TransactionMode]::TransactionalForCatalog;
    }

	$importProgress = $catalogContext.ImportXml($importOptions, $FilePath);
	Write-Host "Importing $($FilePath)";
    while ($importProgress.Status -eq [CommerceServer.Core.Catalog.CatalogOperationsStatus]::InProgress)
    {
        $importProgress.Refresh();
		Write-Progress -PercentComplete $importProgress.PercentComplete -Activity "Importing $($FilePath)" -Status $importProgress.PercentComplete;
    }

	Write-Progress -Activity "Importing $($FilePath)" -Completed -Status 100;

	# If the import operation failed, write the errors to the console.
    if ($importProgress.Status -eq [CommerceServer.Core.Catalog.CatalogOperationsStatus]::Failed)
    {
        foreach ($error in $importProgress.Errors)
        {
            Write-Error string.Format("Line number: $($error.LineNumber) Error message: $($error.Message)" );
        }
    }
    
    $catalogContext.Refresh();
}

function Restart-CSCatalogImportHost
(
	[parameter(Mandatory = $true)][string]$ComApplicationName,
	[string]$ComputerName=$Env:COMPUTERNAME
)
{
	$comAdmin = New-Object -comobject COMAdmin.COMAdminCatalog;
	$apps = $comAdmin.GetCollection("Applications");
	$apps.Populate();
	$app = $apps | Where-Object { $_.Name -eq $ComApplicationName };
	
	if( $app )
	{
		$comAdmin.ShutDownApplication($app.Key);
		$comAdmin.StartApplication($app.Key);
	}
	else
	{
		Write-Error "The application [$($ComApplicationName)] cannot be found.";
	}

}

function Import-MarketingData
{
	PARAM
	(
	    [String]
		$CommerceSiteName=$(throw 'Parameter -CommerceSiteName is missing!'),

	    [String]
		$FilePath=$(throw 'Parameter -FilePath is missing!')
	)

	Write-Host "Running file $IMPORT_MARKETING"
	Write-Host "Importing markering for $CommerceSiteName" -ForegroundColor Green;

	$argumentListReg = @( $CommerceSiteName, """$FilePath""", "-Import" );

	Write-Host $argumentListReg

	$returnValue = Start-Process -FilePath $IMPORT_MARKETING -Wait -NoNewWindow -ArgumentList $argumentListReg -PassThru;

	Write-Host $returnValue.StandardOutput;

	if( $returnValue.ExitCode -ne 0 )
	{
		Write-Error -Message $returnValue.StandardError -ErrorId $returnValue.ExitCode ;
	}
}
