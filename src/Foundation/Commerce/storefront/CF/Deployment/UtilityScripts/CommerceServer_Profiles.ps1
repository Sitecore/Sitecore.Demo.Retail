################################################################
#.Synopsis
#  Exports the Profile Schema from a Commerce Server Site
#.Parameter CommerceSiteName
#  The Commerce Server site name
#.Parameter FilePath
#  The path to the exported xml file.
################################################################
function Export-CS2007ProfileSchema()
{
	PARAM
	(
		[String]$CommerceSiteName=$(throw 'Parameter -CommerceSiteName is missing!'),
		[String]$FilePath=$(throw 'Parameter -FilePath is missing!'),
		[String]$Username="",
		[String]$Password=""
    )
    trap
    {
		Write-Host "Error: $($_.Exception.GetType().FullName)" -ForegroundColor Red ; 
		Write-Host $_.Exception.Message; 
		Write-Host $_.Exception.StackTrack;

		EXIT 1;
    }

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
    
    EXIT 0;
}

################################################################
#.Synopsis
#  Import an exported Profile Schema
#.Parameter CommerceSiteName
#  The Commerce Server site name
#.Parameter FilePath
#  The path to the exported xml file.
################################################################
function Import-CS2007ProfileSchema()
{
    PARAM
    (
		[String]$CommerceSiteName=$(throw 'Parameter -CommerceSiteName is missing!'),
		[String]$FilePath=$(throw 'Parameter -FilePath is missing!'),
		[String]$Username="",
		[String]$Password=""
	)
    trap
    {
		Write-Host "Error: $($_.Exception.GetType().FullName)" -ForegroundColor Red ; 
		Write-Host $_.Exception.Message; 
		Write-Host $_.Exception.StackTrack;

		EXIT 1;
	}

    # Load Assemblies
    Write-Host "Loading .NET Assemblies..."
    [void][System.Reflection.Assembly]::Load("CommerceServer.Core.Profiles, Version=10.0.0.0, Culture=neutral, PublicKeyToken=f5c79cb11734af7a")
    [void][System.Reflection.Assembly]::Load("CommerceServer.Core.Configuration, Version=10.0.0.0, Culture=neutral, PublicKeyToken=f5c79cb11734af7a")
    [void][System.Reflection.Assembly]::Load("ADODB, Version=7.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
    [void][System.Reflection.Assembly]::Load("System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
     
    # Initialize SiteConfigReadOnlyFreeThreaded            
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

	$xml = $doc.get_InnerXml();

	$xml = $xml.Replace("connStr=`"`"", "connStr=`"" + $bdsConnect  + "`"");
     
    # Import Profile Schema
    Write-Host "Importing Profile Schema from " $FilePath
    $bizDataAdmin.ImportCatalogs($xml)
    
    # Release underlying COM resources
    $bizDataAdmin.Dispose()
    Write-Host "Profile Schema Imported Successfully!"
}