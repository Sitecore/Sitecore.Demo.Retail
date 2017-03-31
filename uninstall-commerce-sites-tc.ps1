param([String]$prNumber)
Write-Host 'Ready to modify '  $jsonFile 

$catalogDBName = 'habitat'+ $prNumber + '_CommerceServer.ProductCatalog'
$profileDBName = 'habitat'+ $prNumber + '_CommerceServer.Profiles'


Set-ExecutionPolicy Unrestricted –scope CurrentUser
if (-Not ($PSVersionTable.PSVersion.Major -ge 4)) { Write-Host "Update version of powershell"; exit }

Import-Module $PSScriptRoot\Scripts\Commerce\ManageUser.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\ManageCommerceServer.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\ManageIIS.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\ManageSqlServer.psm1 -Force
cd $PSScriptRoot

$commerceCatalogDB = 

If ((ManageUser\Confirm-Admin) -ne $true) { Write-Host "Please run script as administrator"; exit }

$settings = ((Get-Content $PSScriptRoot\install-commerce-config.json -Raw) | ConvertFrom-Json)

$installFolderSetting = ($settings.resources | Where { $_.id -eq "install" } | Select)
If ($installFolderSetting -eq $null) { Write-Host "Expected install resources" -ForegroundColor red; exit; }
  
# Step 1: Remove Commerce Server SIte
Write-Host "`nStep 1: Remove Commerce Server Site" -foregroundcolor Yellow
if(Get-Module -ListAvailable -Name CSPS) 
{
    Import-Module CSPS -Force
    ManageCommerceServer\Remove-CSWebsite -csSiteSetting $settings.sitecoreCommerce.csSite -Verbose
}
Else
{
    Write-Host "CSPS PoswerShell Module not installed so assuming no commerce site exists"
}
# Step 2: Remove databases from SQL
Write-Host "`nStep 2: Remove databases from SQL" -foregroundcolor Yellow
cd SQLSERVER:\SQL
    
	ManageSqlServer\Remove-Database -dbname $catalogDBName -Verbose
	ManageSqlServer\Remove-Database -dbname $profileDBName -Verbose

	cd $PSScriptRoot

# Step 3: Uninstall websites
Write-Host "`nStep 3: Remove Websites" -foregroundcolor Yellow
Foreach ($website in $settings.iis.websites)
{
	If ( $website.id -ne "sitecore") {
		ManageIIS\Remove-Site -name $website.siteName -Verbose
	}
}

# Step 4: Uninstall Application Pools
Write-Host "`nStep 4: Remove Application Pools" -foregroundcolor Yellow
Foreach ($appPool in $settings.iis.appPools)
{
	If ( $appPool.id -ne "sitecore") {
		ManageIIS\Remove-AppPool -name $appPool.name -Verbose
	}
}

# Step 5: Configure host file
Write-Host "`nStep 5: Configure Host File" -foregroundcolor Yellow
If((ManageIIS\Remove-HostEntries -hostEntryList $settings.iis.hostEntries -Verbose) -ne 0) { Exit }




