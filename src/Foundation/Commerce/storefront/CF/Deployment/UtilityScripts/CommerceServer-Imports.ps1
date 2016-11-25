#Requires -Version 2
$ErrorActionPreference = "Stop";
Set-PSDebug -Strict;

################################################################
#.Synopsis
#  Exports the Profile Schema from a Commerce Server Site
#.Parameter CommerceSiteName
#  The Commerce Server site name
#.Parameter FilePath
#  The path to the exported xml file.
################################################################
function Export-CommerceServerProfileSchema()
{
	PARAM
	(
		[String]$CommerceSiteName=$(throw 'Parameter -CommerceSiteName is missing!'),
		[String]$FilePath=$(throw 'Parameter -FilePath is missing!'),
		[String]$Username="",
		[String]$Password=""
    )

    # If removeCredentials parameter is true, then the credentials (user name and password) will be removed from all connection strings. 
    # These credentials will have to be replaced before the catalog can be imported. 
    # If the catalog is imported through Commerce Server Manager, the user must enter a user name and password for each data source partition in # the catalog. 
    # A value of true indicates remove the credentials. A value of false indicates do not remove the credentials. The default value is true. 
    # If the removeCredentials parameter is false, then the credentials will be exported in plain text with the rest of the catalog.
    # http://msdn.microsoft.com/en-us/library/CommerceServer.Core.interop.profiles.businessdataadmin2freethreaded.exportcatalogs.aspx
    $removeCredentials = 0
     
    # Load Assemblies
    [void][System.Reflection.Assembly]::Load("CommerceServer.Core.Profiles, Version=10.0.0.0, Culture=neutral, PublicKeyToken=f5c79cb11734af7a")
    [void][System.Reflection.Assembly]::Load("CommerceServer.Core.Configuration, Version=10.0.0.0, Culture=neutral, PublicKeyToken=f5c79cb11734af7a")
    [void][System.Reflection.Assembly]::Load("ADODB, Version=7.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
    [void][System.Reflection.Assembly]::Load("System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
     
    # Initialize SiteConfigReadOnlyFreeThreaded            
    Write-Host "Initializing Commerce Server 2007 SiteConfigReadOnly Object"
    $siteConfig = new-object CommerceServer.Core.Configuration.SiteConfigReadOnly
    $siteConfig.Initialize($commerceCommerceSiteName)
     
    # Get the Biz Data Store Connection String (OLEDB)
    $bdsConnect = $siteConfig.Fields.Item("Biz Data Service").Value.Fields.Item("s_BizDataStoreConnectionString").Value
    Write-Host "CS Biz Data Store Connection String (OLEDB): " $bdsConnect
     
    # Release underlying COM resources
    $siteConfig.Dispose()
     
    # Connect to Biz Data Store
    $bizDataAdmin = new-object CommerceServer.Core.Profiles.BusinessDataAdmin2
    Write-Host "Connecting to Biz Data Store"
    $bizDataAdmin.Connect($bdsConnect, $Username, $Password)
     
    # Export to XML File
    Write-Host "Exporting Profile Schema to " $FilePath
    $bizDataAdmin.ExportCatalogs($FilePath, $removeCredentials)
    
    # Release underlying COM resources
    $bizDataAdmin.Dispose()
     
    Write-Host "Profile Schema Exported Successfully!"
}

################################################################
#.Synopsis
#  Import an exported Profile Schema
#.Parameter CommerceSiteName
#  The Commerce Server site name
#.Parameter FilePath
#  The path to the exported xml file.
################################################################
function Import-CommerceServerProfileSchema()
{
    PARAM
    (
		[String]$CommerceSiteName=$(throw 'Parameter -CommerceSiteName is missing!'),
		[String]$FilePath=$(throw 'Parameter -FilePath is missing!'),
		[String]$Username="",
		[String]$Password=""
	)

    # Load Assemblies
    Write-Host "Loading .NET Assemblies..."
    [void][System.Reflection.Assembly]::Load("CommerceServer.Core.Profiles, Version=10.0.0.0, Culture=neutral, PublicKeyToken=f5c79cb11734af7a")
    [void][System.Reflection.Assembly]::Load("CommerceServer.Core.Configuration, Version=10.0.0.0, Culture=neutral, PublicKeyToken=f5c79cb11734af7a")
    [void][System.Reflection.Assembly]::Load("ADODB, Version=7.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
    [void][System.Reflection.Assembly]::Load("System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
     
    # Initialize SiteConfigReadOnly          
    Write-Host "Initializing Commerce Server 2007 SiteConfigReadOnly Object..."
    $siteConfig = new-object CommerceServer.Core.Configuration.SiteConfigReadOnly
    $siteConfig.Initialize($CommerceSiteName)
     
    # Get the Biz Data Store Connection String (OLEDB)
    $bdsConnect = $siteConfig.Fields.Item("Biz Data Service").Value.Fields.Item("s_BizDataStoreConnectionString").Value
    Write-Host "CS Biz Data Store Connection String (OLEDB): " $bdsConnect
     
    # Release underlying COM resources
    $siteConfig.Dispose()
     
    # Connect to Biz Data Store
    $bizDataAdmin = new-object CommerceServer.Core.Profiles.BusinessDataAdmin2
    Write-Host "Connecting to Biz Data Store"
    $bizDataAdmin.Connect($bdsConnect, $Username, $Password)
     
    # Read XML into string
    $doc = new-object System.Xml.XmlDocument
    $doc.Load($FilePath)
     
    # Import Profile Schema
    Write-Host "Importing Profile Schema from " $FilePath
    $bizDataAdmin.ImportCatalogs($doc.get_InnerXml())
    
    # Release underlying COM resources
    $bizDataAdmin.Dispose()
    Write-Host "Profile Schema Imported Successfully!"
}

################################################################
#.Synopsis
#  Import an exported Inventory Schema
#.Parameter CommerceSiteName
#  The Commerce Server site name
#.Parameter FilePath
#  The path to the exported xml file.
################################################################
function Import-CommerceServerInventorySchema
{
	PARAM
	(
	    [String]$CommerceSiteName=$(throw 'Parameter -CommerceSiteName is missing!'),
	    [String]$FilePath=$(throw 'Parameter -FilePath is missing!')
	)

    Write-Host "Loading .NET Assemblies...";
    [void][System.Reflection.Assembly]::Load("CommerceServer.Core.Catalog, Version=10.0.0.0, Culture=neutral, PublicKeyToken=f5c79cb11734af7a")
    [void][System.Reflection.Assembly]::Load("ADODB, Version=7.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
    [void][System.Reflection.Assembly]::Load("System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
    
	Write-Host "Creating a CatalogSiteAgent to connect to the database.";
  	$catalogSiteAgent = New-Object CommerceServer.Core.Catalog.CatalogSiteAgent;
    $catalogSiteAgent.SiteName = $CommerceSiteName;
    $catalogSiteAgent.IgnoreInventorySystem = $false;

	$catalogContext = [CommerceServer.Core.Catalog.CatalogContext]::Create($catalogSiteAgent);

	Write-Host "Getting the Inventory Context";
    $inventoryContext = $catalogContext.InventoryContext;

	$importOptions = New-Object CommerceServer.Core.Inventory.InventoryImportOptions;
    $importOptions.Mode = [CommerceServer.Core.Catalog.ImportMode]::Full;
    $importOptions.TransactionMode = [CommerceServer.Core.Catalog.TransactionMode]::NonTransactional;
    $importOptions.Operation = [CommerceServer.Core.Catalog.ImportOperation]::ValidateAndImport;
	
	$importProgress = $inventoryContext.ImportXml($importOptions, $FilePath);
	Write-Host "Importing $($FilePath)";
	Write-Host "Importing $($FilePath)";
    while ($importProgress.Status -eq [CommerceServer.Core.Catalog.CatalogOperationsStatus]::InProgress)
    {
        $importProgress.Refresh();
		Write-Progress -PercentComplete $importProgress.PercentComplete -Activity "Importing $($FilePath)" -Status $importProgress.PercentComplete;
    }
	
	# If the import operation failed, write the errors to the console.
    if ($importProgress.Status -eq [CommerceServer.Core.Catalog.CatalogOperationsStatus]::Failed)
    {
        foreach ($error in $importProgress.Errors)
        {
            Write-Error string.Format("Line number: $($error.LineNumber) Error message: $($error.Message)" );
        }
    }
}

################################################################
#.Synopsis
#  Import an exported Inventory Schema
#.Parameter CommerceSiteName
#  The Commerce Server site name
#.Parameter FilePath
#  The path to the exported xml file.
################################################################
function Import-CommerceServerCatalogSchema
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
    [void][System.Reflection.Assembly]::Load("CommerceServer.Core.Catalog, Version=10.0.0.0, Culture=neutral, PublicKeyToken=f5c79cb11734af7a")
    [void][System.Reflection.Assembly]::Load("ADODB, Version=7.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
    [void][System.Reflection.Assembly]::Load("System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
    
	Write-Host "Creating a CatalogSiteAgent to connect to the database.";
  	$catalogSiteAgent = New-Object CommerceServer.Core.Catalog.CatalogSiteAgent;
    $catalogSiteAgent.SiteName = $CommerceSiteName;
    $catalogSiteAgent.AuthorizationMode = [CommerceServer.Core.AuthorizationMode]::NoAuthorization;
    $catalogSiteAgent.IgnoreInventorySystem = $false;

	$catalogContext = [CommerceServer.Core.Catalog.CatalogContext]::Create($catalogSiteAgent);

	Write-Host "Getting the Catalog Context";
    $catalogContext = $catalogContext.CatalogContext;

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
	
	# If the import operation failed, write the errors to the console.
    if ($importProgress.Status -eq [CommerceServer.Core.Catalog.CatalogOperationsStatus]::Failed)
    {
        foreach ($error in $importProgress.Errors)
        {
            Write-Error string.Format("Line number: $($error.LineNumber) Error message: $($error.Message)" );
        }
    }
}

function Get-CSVersion
{
	$SKU_STANDARD = 0xCAA5BAAB;
	$SKU_ENTERPRISE = 0x9665781E;
	$SKU_EVAL = 0xB9C73E9E;
	$SKU_DEVELOPER = 0x9AA649D6;
	$VALUE_VALIDATE = 0xDA973ABC; 
	$VALUE_SKU = 0x2237A0DF;
	$VALUE_SKU_UPGRADE = 0x936C4D7A;
	$VALUE_DAYS_LEFT = 0xA1D58F2B;

	$commerceSku = new-object -comobject Commerce.sku;
	$sku = $commerceSku.queryvalue($VALUE_SKU);

	switch( $sku )
	{
        $SKU_DEVELOPER
		{
            return "Developer Edition";
		}
        $SKU_EVAL
		{
            return "Eval Edition";
		}
        $SKU_ENTERPRISE
		{
            return "Enterprise Edition";
		}
        $SKU_STANDARD
		{
            return "Standard Edition";
		}
        default
		{
            return "Unknown -- $($sku)";
		}
	}
}