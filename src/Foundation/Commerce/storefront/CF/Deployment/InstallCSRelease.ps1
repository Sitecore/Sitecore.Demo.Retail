# Arguments
#======================================
# Release#
param(
	[string]$sccBuildNum = $(throw 'Please specify the Sitecore Commerce (commerceServices) revision used: e.g. 1.0.5'),
    [string]$csBuildNum = $(throw 'Please specify the CS Core build number: e.g. 11.4.69'),
    [string]$connectBuildNum = $(throw 'Please specify the Connect build number: e.g. 20160223.1'),
    [string]$csConnectBuildNum = $(throw 'Please specify the CS Connect build number: e.g. 8.2.63'),
    [string]$sfBuildNum = $(throw 'Please specify the Storefront build number: e.g. 8.3.3'),
    [string]$uxSharedBuildNum = $(throw 'Please specify the UX Shared build number: e.g. 8.3.3'),
    [string]$mmBuildNum =  $(throw 'Please specify the Merch Manager build number: e.g. 8.2.60'),
    [string]$coMgrBuildNum = $(throw 'Please specify the Customer & Order Manager revision used: e.g. 8.2.28'),
	[string]$ppMgrBuildNum = $(throw 'Please specify the Pricing & Promotions Manager revision used: e.g. 8.2.7'),
	[string]$sitecoreVer = $(throw 'Please specify the Sitecore version used: e.g. 8.1'),
	[string]$sitecoreRev = $(throw 'Please specify the Sitecore revision used: e.g. 160302'),
    
	[string]$deploy = "Deploy-All",
    [string]$indexer = "lucene",  # solr-remote, solr-local
	[string]$multiSite = "single", # single, multi

    # This is the subpath to each build output. Update this only if you need to deploy a different build type (like a CI or DEV build). Build numbers still need to be specified above.
	[string]$SUBPATH_SCC = "Commerce.Engine.Rel", 
    [string]$SUBPATH_CSCORE = "Commerce.Core.Rel.11.5", 
	[string]$SUBPATH_CONNECT = "Connect.Commerce.Rel.8.3",
	[string]$SUBPATH_CSCONNECT = "CommerceServer.Connect.Rel.8.3",
	[string]$SUBPATH_SF = "Commerce.Storefront.Rel",
    [string]$SUBPATH_UX = "Commerce.UX.Shared.Rel",
	[string]$SUBPATH_MM = "Commerce.MerchandisingManager.Rel",
	[string]$SUBPATH_CO = "Commerce.CustomerOrderManager.Rel",
	[string]$SUBPATH_PP = "Commerce.PricingPromotionsManager.Rel"
)

function Log
{
	param(
		[string]$level = "h1",
		[string]$msg = ""
	)
	
	switch ($level)
	{
		"h1" 
		{ 
			Write-Host ""
			Write-Host "========================================" -ForegroundColor Green ;
			Write-Host "|| $msg" -ForegroundColor Green ;
			Write-Host "========================================" -ForegroundColor Green ;
		}
		"err" 
		{ 
			Write-Host $msg -ForegroundColor Red ;
		}
		default 
		{
			Write-Host $msg -ForegroundColor White ;
		}
	}
}

# Common Variables
#======================================
$LOG_FILE = "C:\InstallCSRelease.log"

$CS_PATH = "C:\CSFiles"
$CS_INSTANCE = "CSTest"
$CS_CONFIG_FILE = "CSConfig.xml"
$CS_CONFIG_PATH = "$CS_PATH\$CS_CONFIG_FILE"

$CS_INSTALL_FILE = "InstallCS.bat"
$CS_INSTALL_PATH = "$CS_PATH\$CS_INSTALL_FILE"

$CS_SERVICE_PREFIX = "CFSolutionStorefrontSite"
$CS_SERVICES_PATH_PREFIX = "C:\inetpub\CFCSServices\$CS_SERVICE_PREFIX"

$CS_TOOLS_PATH = "C:\Program Files (x86)\Commerce Server 11\Tools"

$SC_PATH = "\\fil1ca2\Data\Downloads\Sitecore\$sitecoreVer" 
$SC_LICENSE_PATH = "$SC_PATH\license.xml"

$SC_SITE_PATH = "C:\inetpub\$CS_INSTANCE"
$SC_SITE_WEBSITE = "$SC_SITE_PATH\Website"
$SC_SITE_WEBSITE_BIN = "$SC_SITE_WEBSITE\bin"

$INSTALL_PATH = "C:\Deploy" 
$INSTALL_PATH_DATABASE = "$INSTALL_PATH\Database"
$INSTALL_PATH_DEPLOYMENT = "$INSTALL_PATH\Deployment"
$INSTALL_PATH_MODULES = "$INSTALL_PATH\Modules"
$INSTALL_PATH_PACKAGES = "$INSTALL_PATH\Packages"
$INSTALL_PATH_SITECORE = "$INSTALL_PATH\Sitecore"
$INSTALL_PATH_ZIPS = "$INSTALL_PATH\Zips"

# These are the build paths. They should not change unless significant changes are done to the projects and the build output.
# Use the subpaths in paramters above to manipulate if you want to use a different build defintion
$BUILD_PATH = "\\fil1ca2\builds01"
#$BUILD_PATH_DK = "\\fil1dk1\PD-CI\Builds"

$BUILD_PATH_SCC = "$BUILD_PATH\$SUBPATH_SCC\$sccBuildNum"
$BUILD_PATH_CSCORE = "$BUILD_PATH\$SUBPATH_CSCORE\$csBuildNum"
$BUILD_PATH_CONNECT = "$BUILD_PATH\$SUBPATH_CONNECT\$connectBuildNum"
$BUILD_PATH_CSCONNECT = "$BUILD_PATH\$SUBPATH_CSCONNECT\$csConnectBuildNum"
$BUILD_PATH_SF = "$BUILD_PATH\$SUBPATH_SF\$sfBuildNum"
$BUILD_PATH_UX = "$BUILD_PATH\$SUBPATH_UX\$uxSharedBuildNum"
$BUILD_PATH_MM = "$BUILD_PATH\$SUBPATH_MM\$mmBuildNum"
$BUILD_PATH_CO = "$BUILD_PATH\$SUBPATH_CO\$coMgrBuildNum"
$BUILD_PATH_PP = "$BUILD_PATH\$SUBPATH_PP\$ppMgrBuildNum"

$DEP_PATH = "\\fil1ca2\Test\BuildDependencies"

function Set-Deployment-Location
{
	Push-Location $INSTALL_PATH_DEPLOYMENT
}

function Unset-Deployment-Location
{
	Pop-Location
}

function Create-Directory-Structure
{
	Log -level h1 -msg "Create-Directory-Structure"
	# Create directory structure
	#======================================
	# C:\Deploy
	#          \Database
	#          \Deployment
	#          \Modules
	#          \Packages
	#          \Sitecore
    #          \Zips
	New-Item -ItemType directory -Path $INSTALL_PATH
    New-Item -ItemType directory -Path $INSTALL_PATH_DATABASE
    New-Item -ItemType directory -Path $INSTALL_PATH_DEPLOYMENT
	New-Item -ItemType directory -Path $INSTALL_PATH_MODULES
	New-Item -ItemType directory -Path $INSTALL_PATH_PACKAGES
	New-Item -ItemType directory -Path $INSTALL_PATH_SITECORE
    New-Item -ItemType directory -Path $INSTALL_PATH_ZIPS
}

function Copy-Release-Files
{
	Log -level "h1" -msg "Copy-Release-Files"
    Log -level default -msg "Sitecore Commerce Path $BUILD_PATH_SCC"
    Log -level default -msg "CS Core Path $BUILD_PATH_CSCORE"
    Log -level default -msg "Connect Path $BUILD_PATH_CONNECT"
    Log -level default -msg "CS Connect Path $BUILD_PATH_CSCONNECT"
    Log -level default -msg "SF Path $BUILD_PATH_SF"
    Log -level default -msg "UX Shared Path $BUILD_PATH_UX"
    Log -level default -msg "MM Path $BUILD_PATH_MM"
    Log -level default -msg "CO Path $BUILD_PATH_CO"
    Log -level default -msg "PP Path $BUILD_PATH_PP"
    
    # Copy deployment scripts
	Copy-Item "$BUILD_PATH_SF\CF\Database" -destination $INSTALL_PATH -recurse -container -force
	Copy-Item "$BUILD_PATH_SF\CF\Deployment" -destination $INSTALL_PATH -recurse -container -force

    # Get some things from Nuget
    Copy-Item "$DEP_PATH\nuget.exe" -Destination $INSTALL_PATH_DEPLOYMENT -force
    Copy-Item "$DEP_PATH\NuGet.config" -Destination $INSTALL_PATH_DEPLOYMENT -force
    Copy-Item "$INSTALL_PATH_DEPLOYMENT\packagesQA.config" "$INSTALL_PATH_DEPLOYMENT\packages.config" -force # We have to use "packages.config" for some stupid reason so overwrite the dev one
    & "$INSTALL_PATH_DEPLOYMENT\nuget.exe" restore "$INSTALL_PATH_DEPLOYMENT\packages.Config" -PackagesDirectory "$INSTALL_PATH_DEPLOYMENT\Packages" -Config "$INSTALL_PATH_DEPLOYMENT\NuGet.config"

    # Copy SCC
    Copy-Item "$BUILD_PATH_SCC\bin\*.update" -destination $INSTALL_PATH_PACKAGES -recurse -container -force
    Copy-Item "$BUILD_PATH_SCC\bin\Sitecore.Commerce.Engine*.zip" -destination $INSTALL_PATH_ZIPS -recurse -container -force
    Copy-Item "$BUILD_PATH_SCC\bin\CommerceServicesDbScript.sql" -destination $INSTALL_PATH_ZIPS -recurse -container -force

    # Copy CS Core
    Copy-Item "$BUILD_PATH_CSCORE\binaries\CommerceServer-*.exe" -destination $INSTALL_PATH -force
    
    # Copy Connect
    Copy-Item "$BUILD_PATH_CONNECT\output\Installation Packages\Sitecore Commerce Connect 8*.zip" -destination $INSTALL_PATH_MODULES -force

    # Copy CS Connect
    Copy-Item "$BUILD_PATH_CSCONNECT\App\TDSProject_Master\Package_Release\*.update" -destination $INSTALL_PATH_PACKAGES -recurse -force

    # Copy Storefront Common Files
    Copy-Item "$BUILD_PATH_SF\Common\TDSCommon_Master\Package_Release\*.update" -destination $INSTALL_PATH_PACKAGES -recurse -force

    # Copy Storefront CS Files
	Copy-Item "$INSTALL_PATH_DEPLOYMENT\Packages\Adventure.Works.Images.*\content\Adventure Works Images.zip" -destination $INSTALL_PATH_MODULES -force
    Copy-Item "$INSTALL_PATH_DEPLOYMENT\Packages\Sitecore.Helix.Images.*\content\Sitecore.Helix.Images-*.zip" -destination $INSTALL_PATH_MODULES -force
	Copy-Item "$BUILD_PATH_SF\CF\TDSCommerceEngine_Master\Package_Release\*.update" -destination $INSTALL_PATH_PACKAGES -recurse -force
    
    # Copy UX Shared 
    Copy-Item "$BUILD_PATH_UX\CommerceUI_Shared_Core\bin\Package_Release\*.update" -destination $INSTALL_PATH_PACKAGES -recurse -force 

    # Copy MM
    Copy-Item "$BUILD_PATH_MM\Biz_Core\Package_Release\*.update" -destination $INSTALL_PATH_PACKAGES -recurse -force  

    # Copy SPEAK Modules for CO and PP
    #Copy-Item "\\fil1ca2\Dependencies\Speak\Sitecore Speak*.zip" -destination "$INSTALL_PATH_MODULES" -force

    # Copy CO
    Copy-Item "$BUILD_PATH_CO\CustomerOrderManager_Core\Package_Release\*.update" -destination $INSTALL_PATH_PACKAGES -recurse -force 

    # Copy PP
    Copy-Item "$BUILD_PATH_PP\TDS_Core\Package_Release\*.update" -destination $INSTALL_PATH_PACKAGES -recurse -force 

    # Copy Sitecore
    $sitecoreFile = "Sitecore $sitecoreVer rev. $sitecoreRev"
	$srcPath = "$SC_PATH\$sitecoreFile"
    $sitecoreFilePath = "$srcPath\$sitecoreFile.zip"
    $solrFile = Get-ChildItem "$srcPath\Sitecore.Solr.Support*.zip" | %{$_.Name}
    $solrFilePath = "$srcPath\$solrFile"
	
	# Copy files to deployment
	Copy-Item $sitecoreFilePath -destination $INSTALL_PATH_SITECORE -force
    Copy-Item $solrFilePath -destination $INSTALL_PATH_SITECORE -force
}

function Prepare-Deployment-Items
{
	Log -level h1 -msg "Prepare-Deployment-Items"
	# Copy CSConfig.xml file for CS installation
	Copy-Item $CS_CONFIG_PATH -destination $INSTALL_PATH -force
	
	# Copy license file to Sitecore folder
	Copy-Item "$SC_LICENSE_PATH" -destination $INSTALL_PATH_SITECORE -force
}

function Clean-SC-Deployment
{
	Log -level "h1" -msg "Clean-SC-Deployment"
	Set-Deployment-Location
    . "$INSTALL_PATH_DEPLOYMENT\Deploy-CleanCommerceServices.ps1"
	. "$INSTALL_PATH_DEPLOYMENT\CleanUpSitecoreInstall.ps1"
    . "$INSTALL_PATH_DEPLOYMENT\Deploy-Mongo-Clean.ps1"
	Unset-Deployment-Location
}

function Install-CS
{
	Log -level "h1" -msg "Install-CS"
    $CS_EXE = Get-ChildItem "$INSTALL_PATH\CommerceServer-*.exe" | %{$_.Name}
	cmd.exe /c $INSTALL_PATH_DEPLOYMENT\$CS_INSTALL_FILE $INSTALL_PATH\$CS_EXE $INSTALL_PATH\$CS_CONFIG_FILE
}

function Update-Environment-Variables
{
    Log -level "h1" -msg "Update-Environment-Variables"

    $locations = 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
                 'HKCU:\Environment'

    $locations | ForEach-Object {   
        $k = Get-Item $_
        $k.GetValueNames() | ForEach-Object {
            $name  = $_
            $value = $k.GetValue($_)
            
            if (($name -eq "COMMERCE_SERVER_ROOT") -or ($name -eq "COMMERCE_SERVER_ROOT_64") ) {
                Set-Item -Path Env:\$name -Value $value
                Log -msg "VAR: $name = $value"
            }
        }
    }
}

function Unpup-CS
{
	Log -level "h1" -msg "Unpup-CS"
	Set-Deployment-Location
	. "$INSTALL_PATH_DEPLOYMENT\Deploy-UnPup.ps1"
	Unset-Deployment-Location
}

function Update-Environment
{
	Log -level "h1" -msg "Update-Environment"
	Set-Deployment-Location
	. "$INSTALL_PATH_DEPLOYMENT\UpdateEnvironmentAndConfigFiles.ps1"
	Unset-Deployment-Location
}

function Deploy-SC-CS
{
	Log -level "h1" -msg "Deploy-SC-CS"
	Set-Deployment-Location
    . "$INSTALL_PATH_DEPLOYMENT\Deploy-DeployCommerceServices.ps1"
    . "$INSTALL_PATH_DEPLOYMENT\Deploy-BootstrapCommerceServices.ps1"
	. "$INSTALL_PATH_DEPLOYMENT\Deploy-Base-SC.ps1"    
	. "$INSTALL_PATH_DEPLOYMENT\Deploy-CopyFiles.ps1"
	#. "$INSTALL_PATH_DEPLOYMENT\Deploy-DeleteSpeakComponents.ps1"
	. "$INSTALL_PATH_DEPLOYMENT\Deploy-InstallModules.ps1"
	. "$INSTALL_PATH_DEPLOYMENT\Deploy-InstallPackages.ps1"
    . "$INSTALL_PATH_DEPLOYMENT\Deploy-UpdateConfigs.ps1"
	. "$INSTALL_PATH_DEPLOYMENT\Deploy-Base-SC-Merge.ps1"  
    . "$INSTALL_PATH_DEPLOYMENT\Deploy-InitializeCommerceServices.ps1"
    
	if ($indexer -eq "solr-remote" -or $indexer -eq "solr-local")
    {
        Deploy-Solr
    }

	Post-Deploy-CS

	if ($multiSite -eq "multi")
    {
        Deploy-MultiSite
    }

	iisreset
	. "$INSTALL_PATH_DEPLOYMENT\Deploy-PublishToWeb.ps1"
	. "$INSTALL_PATH_DEPLOYMENT\Deploy-RebuildIndices.ps1"
	Unset-Deployment-Location
}

function Add-Machine-Binding
{
	Log -level "h1" -msg "Add-Machine-Binding"
	$cmd = "$env:windir\system32\inetsrv\appcmd.exe"
	Log -msg "APPCMD=$cmd"
	.$cmd set site /site.name: $CS_INSTANCE /+"bindings.[protocol='http',bindingInformation='*:80:$env:COMPUTERNAME']"
    .$cmd set site /site.name: $CS_INSTANCE /+"bindings.[protocol='https',bindingInformation='*:443:$env:COMPUTERNAME']"
}

function Set-AppPool-Identity
{
	Log -level "h1" -msg "Set-AppPool-Identity"
	
	Import-Module -Name WebAdministration;
	$apfullname = 'CFSolutionStorefrontSiteAppPool_Foundation';
	$username = 'CommerceQA-SVC';
	$password = 'sitecoreGA!12';
	
	$appPoolInstance = Get-ChildItem "IIS:\AppPools" | where { $_.Name -eq $($apfullname) };
	$appPoolInstance.processModel.identityType = 3;
	$appPoolInstance.processModel.userName = $($username);
	$appPoolInstance.processModel.password = $($password);
	$appPoolInstance | Set-Item;
}

function Deploy-MultiSite
{
    Log -level "h1" -msg "Deploy-MultiSite"
    . "$INSTALL_PATH_DEPLOYMENT\Deploy-NewStorefront.ps1"
}

function Deploy-Solr
{
    Log -level "h1" -msg "Deploy-Solr"

    # Toggle Search Provider
    Log -msg "Toggling Search Provider from Lucene to Solr"
    . "$INSTALL_PATH_DEPLOYMENT\ToggleSearchProvider.ps1"

    # Unzip Solr Package
    Log -msg "Unzipping Solr Package to website\bin"
    $shell = new-object -com shell.application
    $zipFile = "$INSTALL_PATH_SITECORE\Sitecore.Solr.Support*.zip"
	$folder = "bin"
	$item = $shell.NameSpace("$zipFile\$folder")
	$shell.Namespace($SC_SITE_WEBSITE).copyhere($item, 0x14) # 0x14 = Overwrite

    # Reset SOLR buckets
    if ($indexer -eq "solr-remote")
    {
        $computerName = $env:COMPUTERNAME.ToLower()
        $solrServer = "http://WS-SVC-SOLR1:8080/solr-$computerName-$sitecoreVer"

        $SOLR_BUCKETS_81 = "fxm", "itembuckets_commerce_products_master_index", "itembuckets_commerce_products_web_index", "marketingdefinitions", "sitecore_analytics_index", "sitecore_core_index", "sitecore_fxm_master_index", "sitecore_fxm_web_index", "sitecore_list_index", "sitecore_marketing_asset_index_master", "sitecore_marketing_asset_index_web", "sitecore_master_index", "sitecore_suggested_test_index", "sitecore_testing_index", "sitecore_web_index", "social_messages_master", "social_messages_web"

        Foreach ($bucket in $SOLR_BUCKETS_81)
        {
            try
            {
                Invoke-WebRequest -Uri "$solrServer/$bucket/update?stream.body=<delete><query>*:*</query></delete>"
            }
            catch
            {
                Log -level "err" -msg "Deleting index for $solrServer/$bucket Failed"
                Log -level "err" -msg $_.Exception.ToString()
            }
        }

        # Change environment variable to point to SOLR server
        $storefrontConfigPath = "$SC_SITE_WEBSITE\App_Config\Include\Reference.Storefront\Reference.Storefront.config"
        $storefrontConfig = [xml] (Get-Content $storefrontConfigPath)

        $ns = @{ patch = "http://www.sitecore.net/xmlconfig/" }
        $node = Select-Xml -xml $storefrontConfig  -XPath "/configuration/sitecore/settings/setting[@name='ContentSearch.Solr.ServiceBaseAddress']/patch:attribute" -Namespace $ns
        $node.Node.'#text' = $solrServer
        $storefrontConfig.Save($storefrontConfigPath)
    }
}

function Post-Deploy-Copy-Tools
{
	Log -level "h1" -msg "Post-Deploy-Copy-Tools"

	# ProfileImport.exe is not part of the installers now, it will be provided as a separate download
	# Because of this, we need to get the exe from the $RELEASE_PATH\Released dir and place it in the Website\bin dir
	Copy-Item "$BUILD_PATH_CSCONNECT\ProfileImport\bin\ProfileImport.exe" -destination $SC_SITE_WEBSITE_BIN -force
}

function Set-Encryption-Keys-ProfileWebService
{
	Log -level "h1" -msg "Set-Encryption-Keys-ProfileWebService"
	$webConfig = $CS_SERVICES_PATH_PREFIX + '_ProfilesWebService\Web.config'
	$doc = (Get-Content $webConfig) -as [Xml]
	$node = $doc.configuration.CommerceServer.profilesWebService | where {$_.siteName -eq 'CFSolutionStorefrontSite'}
	$node.SetAttribute("publicKey", "registry:HKEY_LOCAL_MACHINE\SOFTWARE\CommerceServer\Encryption\Keys\CFSolutionStorefrontSite,PublicKey")
	$node.SetAttribute("keyIndex", "1")
	$node.SetAttribute("privateKey1", "registry:HKEY_LOCAL_MACHINE\SOFTWARE\CommerceServer\Encryption\Keys\CFSolutionStorefrontSite,PrivateKey")
	$doc.Save($webConfig)
}

function Post-Deploy-CS
{
    Log -level "h1" -msg "Post-Deploy-CS"

    . ( Join-Path -Path $INSTALL_PATH_DEPLOYMENT -ChildPath "UtilityScripts\File.ps1");
    # Update Storefront Config
    $storefrontConfigPath = "$SC_SITE_WEBSITE\App_Config\Include\Reference.Storefront\Reference.Storefront.config"
    Update-XmlAttribute -FilePath $storefrontConfigPath -XPath "/configuration/sitecore/sites/site[@name='storefront']" -UpdateAttribute -AttributeName "hostName" -AttributeValue $env:COMPUTERNAME -Namespaces $null;
}

function Deploy-Common
{
	Log -level "h1" -msg "Redeploy-Common"
	Update-Environment
	Install-CS
	Update-Environment-Variables
	Unpup-CS
	Deploy-SC-CS
	Post-Deploy-Copy-Tools
	Set-Encryption-Keys-ProfileWebService
	Add-Machine-Binding
    Set-AppPool-Identity
	iisreset
}

function Deploy-All
{
	Log -level "h1" -msg "Deploy-All"
	Deploy-CopyFilesOnly
	Deploy-Common
}

function Deploy-CopyFilesOnly
{
    Log -level "h1" -msg "Deploy-CopyFilesOnly"
	Create-Directory-Structure
	Copy-Release-Files
	Prepare-Deployment-Items
}

function Redeploy-SC-Only
{
	Log -level "h1" -msg "Redeploy-SC-Only"
	Clean-SC-Deployment
	Deploy-SC-CS
	Post-Deploy-Copy-Tools
	Set-Encryption-Keys-ProfileWebService
	Add-Machine-Binding
    Set-AppPool-Identity
	iisreset
}

# Main
#==========================================================
# IGNORE_TRANSCRIPT is used to indicate to other called scripts, e.g. Deploy.ps1,
# to ignore starting/stopping of their own transcription, such that there will only
# be a single log created.
$IGNORE_TRANSCRIPT = ""

Start-Transcript -path $LOG_FILE -append
Log -level h1 -msg "Installing CS"
Log

if (Get-Command $deploy -errorAction SilentlyContinue)
{
    . $deploy
}
else
{
	Log -level "err" -msg "Unknown deployment command: $deploy"
}
Stop-Transcript