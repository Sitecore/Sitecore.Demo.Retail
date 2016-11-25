#Requires -Version 3

# Common Configurations
$deployConfig="DevDeploy"
$installDir = "C:\inetpub\CFRefStorefront"
$scHostHeaderName = "cf.reference.storefront.com"
$urlBase = "http://" + $scHostHeaderName + "/SiteUtilityPages"
$projDependenciesDir = $COMMERCESERVICES_DEPENDENCIES_DIRECTORY

# Sitecore Commerce Services databases
$commerceServicesDbName = "SitecoreCommerce_SharedEnvironments"
$commerceServicesGlobalDbName = "SitecoreCommerce_Global"

# Sitecore Commerce OpsApi Services
$urlCommerceOpsApiServicesBootstrap = "http://localhost:5015/commerceops/Bootstrap()"
$urlCommerceOpsApiServicesInitializeEnvironment = "http://localhost:5015/commerceops/InitializeEnvironment(environment='CommerceOps')"
$urlCommerceOpsApiServicesClean = "http://localhost:5015/commerceops/CleanEnvironment()"
$urlCommerceOpsApiServicesImportEnvironment = "http://localhost:5015/commerceops/ImportEnvironment()"

$commerceOpsApiServicesSiteName = "CommerceOps"
$commerceOpsApiServicesAppPoolName = "CommerceOps"
$commerceOpsApiServicesEnvironmentName = "CommerceOps"
$commerceOpsApiServicesPort = 5015

# Sitecore Commerce Shops Services
$urlCommerceShopsServicesBootstrap = "http://localhost:5000/commerceops/Bootstrap()"
$urlCommerceShopsServicesInitializeEnvironment = "http://localhost:5000/commerceops/InitializeEnvironment(environment='AdventureWorksShops')"
$urlCommerceShopsServicesClean = "http://localhost:5000/commerceops/CleanEnvironment()"
$urlCommerceShopsServicesImportEnvironment = "http://localhost:5000/commerceops/ImportEnvironment()"

$commerceShopsServicesSiteName = "CommerceShops"
$commerceShopsServicesAppPoolName = "CommerceShops"
$commerceShopsServicesEnvironmentName = "AdventureWorksShops"
$commerceShopsServicesPort = 5000

# AdventureWorks Initialize Environment URL (legacy)
$urlAdventureWorksInitializeEnvironment = "http://localhost:5000/commerceops/InitializeEnvironment(environment='AdventureWorks')"

# Sitecore Commerce Authoring Services
$urlCommerceAuthoringServicesBootstrap = "http://localhost:5005/commerceops/Bootstrap()"
$urlCommerceAuthoringServicesInitializeEnvironment = "http://localhost:5005/commerceops/InitializeEnvironment(environment='AdventureWorksAuthoring')"
$urlCommerceAuthoringServicesClean = "http://localhost:5005/commerceops/CleanEnvironment()"
$urlCommerceAuthoringServicesImportEnvironment = "http://localhost:5005/commerceops/ImportEnvironment()"

$commerceAuthoringServicesSiteName = "CommerceAuthoring"
$commerceAuthoringServicesAppPoolName = "CommerceAuthoring"
$commerceAuthoringServicesEnvironmentName = "AdventureWorksAuthoring"
$commerceAuthoringServicesPort = 5005

# Sitecore Commerce Minions Services
$urlCommerceMinionsServicesBootstrap = "http://localhost:5010/commerceops/Bootstrap()"
$urlCommerceMinionsServicesInitializeEnvironment = "http://localhost:5010/commerceops/InitializeEnvironment(environment='AdventureWorksMinions')"
$urlCommerceMinionsServicesClean = "http://localhost:5010/commerceops/CleanEnvironment()"
$urlCommerceMinionsServicesImportEnvironment = "http://localhost:5010/commerceops/ImportEnvironment()"

$commerceMinionsServicesSiteName = "CommerceMinions"
$commerceMinionsServicesAppPoolName = "CommerceMinions"
$commerceMinionsServicesEnvironmentName = "AdventureWorksMinions"
$commerceMinionsServicesPort = 5010

# Modules
$modulesDependenciesDir = "$projDependenciesDir\Zips"
$packagesDependenciesDir = "$projDependenciesDir\Packages"
$modulesDirSrc = ".\packages\Sitecore.Commerce.Connect.*\content\Sitecore Commerce Connect*.zip"
$modulesDirDst = $installDir + "\Data\packages"
$advWorksImages = ".\packages\Adventure.Works.Images.*\content\Adventure Works Images.zip"
$helixImages = ".\packages\Sitecore.Helix.Images.*\content\Sitecore.Helix.Images-*.zip"

# Speak Components
#$speakComponents = "$modulesDependenciesDir\Sitecore Speak*.zip"

$packagesAppSrc = ".\packages\Sitecore.CommerceServer.Connect.*\content\Sitecore Commerce Server Connect*.update"
$packagesMerchManagerSrc = ".\packages\Sitecore.Merchandising.Manager.*\content\Sitecore Merchandising Manager*.update"
$packagesVnextSrc = ".\packages\Sitecore.Commerce.Engine.Connect.*\content\Sitecore.Commerce.Engine.Connect*.update"
$packagesStorefrontCommonSrc = "..\..\Common\TDSCommon_Master\Package_Debug\Sitecore.Reference.Storefront.Common*.update"
$packagesMvcSiteSrc = "..\TDSCommerceEngine_Master\Package_Debug\Sitecore.Reference.Storefront.Powered.by.SitecoreCommerce*.update"
$packagesDirSrc = $installDir + "\Website\sitecore\admin\Packages"
$packagesQAPackages = "..\Packages\*.update"
$packagesDirDst = $installDir + "\Website\sitecore\admin\Packages"
$siteUtilitiesDirSrc = "SiteUtilityPages"
$siteUtilitiesDirDst = $installDir + "\Website"
$includeDir = $installDir + "\Website\App_Config\Include"