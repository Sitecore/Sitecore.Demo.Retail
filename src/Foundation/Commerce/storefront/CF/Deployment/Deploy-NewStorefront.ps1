#Requires -Version 3
. .\Deploy-Env-Common.ps1
. .\UtilityScripts\Windows.ps1
. .\BaseFunctions.ps1
. .\UtilityScripts\CommerceServer_Catalog.ps1

$DEPLOYMENT_DIRECTORY=(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent);

LoadEnvironmentXml -ConfigurationIdentity "Domain.Dev.SC";

########################### BEFORE RUNNING SCRIPT VERIFY THESE SETTINGS ##########################################
if ($deployConfig -match "DevDeploy")
{
	#### DEV DEPLOYMENT SETTINGS ####
	$StorefrontNewName = "storefront2";
	$StorefontNewPath = "/sitecore/content/$StorefrontNewName";
	$NewCatalogName = "Adventure Works Catalog"; # TODO Use "Test Catalog" once properly craeted
	$NewCatalogDatasourcePath = "/sitecore/Commerce/Catalog Management/Catalogs/Adventure Works Catalog/Departments";
	$HostName = "cs.reference.$StorefrontNewName.com"
	$CatalogImportPath = (Join-Path -Path $DEPLOYMENT_DIRECTORY -ChildPath "\TestData\Test Catalog.xml");
}
else
{
	#### QA DEPLOYMENT SETTINGS ####
	$StorefrontNewName = "storefront2";
	$StorefontNewPath = "/sitecore/content/$StorefrontNewName";
	$NewCatalogName = "Adventure Works Catalog"; # TODO Use "Test Catalog" once properly craeted
	$NewCatalogDatasourcePath = "/sitecore/Commerce/Catalog Management/Catalogs/Adventure Works Catalog/Departments";
	$HostName = "reference.$StorefrontNewName.com"
	$CatalogImportPath = (Join-Path -Path $DEPLOYMENT_DIRECTORY -ChildPath "\TestData\Test Catalog.xml");
}
##################################################################################################################

############################ DO NOT UPDATE BELOW THIS LINE #######################################################

##### STEP 1: Import the catalog
# TODO - Once we have a catalog to import
#Write-Host "Importing $CatalogImportPath" -ForegroundColor Yellow;
#Import-CS2007CatalogSchema -CommerceSitename $PROJECT_NAME -FilePath $CatalogImportPath -Replace $TRUE -Transacted $TRUE -AllowSchemaUpdate $TRUE;

##### STEP 2: Deploy the storefront
Write-Host "Deploying test storefront..." -ForegroundColor Yellow;
$urlPublish = $urlBase + "/DeployNewStorefront.aspx?name=$StorefrontNewName&catalogname=$NewCatalogName&catalogdatasourcepath=`"$NewCatalogDatasourcePath`""
Invoke-RestMethod $urlPublish -TimeoutSec 1200

##### STEP 3: Copy Views and Scripts
Write-Host "Updating website folder content" -ForegroundColor Yellow;
	
$scriptsFolderPath = Join-Path -Path $SitecoreWebsiteFolder -ChildPath "Scripts";
$viewsFolderPath = Join-Path -Path $SitecoreWebsiteFolder -ChildPath "Views";

$storefrontScriptsFolderPath = Join-Path -Path $scriptsFolderPath -ChildPath "Storefront";
$storefrontViewsFolderPath = Join-Path -Path $viewsFolderPath -ChildPath "Storefront";

$newScriptsFolderPath = Join-Path -Path $scriptsFolderPath -ChildPath $StorefrontNewName;
$newViewsFolderPath = Join-Path -Path $viewsFolderPath -ChildPath $StorefrontNewName;
	
# Deleting folders if they already exist
if(Test-Path $newScriptsFolderPath)
{
	Remove-Item $newScriptsFolderPath -Recurse -Force;
}

if(Test-Path $newViewsFolderPath)
{
	Remove-Item $newViewsFolderPath -Recurse -Force;
}

# Creating folders
Copy-Item $storefrontScriptsFolderPath $newScriptsFolderPath -Recurse -Force;
Copy-Item $storefrontViewsFolderPath $newViewsFolderPath -Recurse -Force;

##### STEP 4: Add the site to the storefront configuration
Write-Host "Adding sitecore site definition" -ForegroundColor Yellow;

$storefrontConfigDoc = New-Object System.Xml.XmlDocument
$storefrontConfigPath = "$SitecoreWebsiteFolder\App_Config\Include\Reference.Storefront\Reference.Storefront.config"

$storefrontConfigDoc.Load($storefrontConfigPath)

$connectionStrings = $storefrontConfigDoc.SelectSingleNode("//configuration//sitecore//sites");	
$storefrontnode = $storefrontConfigDoc.SelectSingleNode("//configuration//sitecore//sites//site[@name='storefront']");
$newstorefrontnode = $storefrontConfigDoc.SelectSingleNode("//configuration//sitecore//sites//site[@name='$StorefrontNewName']");

if ($newstorefrontnode -eq $null)
{
	$newNode = $storefrontnode.Clone();
	$nameAttribute = $newNode.Attributes.GetNamedItem("name")
	$nameAttribute.Value = $StorefrontNewName;
	$rootPathAttribute = $newNode.Attributes.GetNamedItem("rootPath")
	$rootPathAttribute.Value = $StorefontNewPath;       
    $hostNameAttribute = $newNode.Attributes.GetNamedItem("hostName")
	$hostNameAttribute.Value = $HostName;     
	   
    $connectionStrings.AppendChild($newNode);

	$storefrontConfigDoc.Save($storefrontConfigPath)
}
						
##### STEP 5: Add the binding and host file change
Write-Host "Updating hosts file" -ForegroundColor Yellow;
Windows-UpdateHostsFile "127.0.0.1" $HostName

Write-Host "Updating bindings" -ForegroundColor Yellow;
$existingBinding1Config = "*:80:$($HostName)";
$existingBinding1 = Get-WebBinding | where {$_.bindingInformation -eq $existingBinding1Config -and $_.protocol -eq "http"};	
$existingBinding2Config = "*:443:$($HostName)";						
$existingBinding2 = Get-WebBinding | where {$_.bindingInformation -eq $existingBinding2Config -and $_.protocol -eq "https"};							
	
# Deleting bindings						
if($existingBinding1 -ne $null) {
	(Get-WebBinding | where {$_.bindingInformation -eq $existingBinding1Config -and $_.protocol -eq "http"}) | Remove-WebBinding;
}			
					
if($existingBinding2 -ne $null) {
	(Get-WebBinding | where {$_.bindingInformation -eq $existingBinding2Config -and $_.protocol -eq "https"}) | Remove-WebBinding;
}
		
# Creating bindings
New-WebBinding -Name $instanceName -Port 80 -HostHeader $HostName -Protocol http -IP *;
New-WebBinding -Name $instanceName -Port 443 -HostHeader $HostName -Protocol https -IP *;

# NOTE: Publishing and Indexing are not done on purpose in this script due to it impacting QA deployments as part of an integrated deployment
iisreset