Set-ExecutionPolicy Unrestricted –scope CurrentUser
if (-Not ($PSVersionTable.PSVersion.Major -ge 4)) { Write-Host "Update version of powershell"; exit }

Import-Module $PSScriptRoot\Scripts\Commerce\ManageUser.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\ManageCommerceServer.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\ManageIIS.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\ManageSqlServer.psm1 -Force
cd $PSScriptRoot

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

# Step 2: Uninstall Commerce Server
Write-Host "`nStep 2: Uninstall Commerce Server" -foregroundcolor Yellow
ManageCommerceServer\Uninstall-CS -installFolderSetting $installFolderSetting -Verbose

# Step 3: Remove databases from SQL
Write-Host "`nStep 3: Remove databases from SQL" -foregroundcolor Yellow
cd SQLSERVER:\SQL
ForEach ($database in $settings.databases)
{
    ManageSqlServer\Remove-Database -dbname $database.name -Verbose
}
cd $PSScriptRoot

# Step 4: Uninstall websites
Write-Host "`nStep 4: Remove Websites" -foregroundcolor Yellow
Foreach ($website in $settings.iis.websites)
{
    ManageIIS\Remove-Site -name $website.siteName -Verbose
}

# Step 5: Uninstall Application Pools
Write-Host "`nStep 5: Remove Application Pools" -foregroundcolor Yellow
Foreach ($appPool in $settings.iis.appPools)
{
    ManageIIS\Remove-AppPool -name $appPool.name -Verbose
}

# Step 6: Remove User
Write-Host "`nStep 6: Remove Users" -foregroundcolor Yellow
#$runTimeUserSetting = ($settings.accounts | Where { $_.id -eq "runTime" } | Select)
#ManageUser\Remove-User -username $runTimeUserSetting.username -Verbose

# Step 7 Remove sql login
Write-Host "`nStep 7: Remove Sql Login" -foregroundcolor Yellow
ManageSqlServer\Remove-SqlLogin -accountId "runTime" -databaseId "commerceAdminDB" -accountSettingList $settings.accounts -databaseSettingList $settings.databases -Verbose

# Step 8 Clean up hosts file
Write-Host "`nStep 8: Configure Host File" -foregroundcolor Yellow
If((ManageIIS\Remove-HostEntries -hostEntryList $settings.iis.hostEntries -Verbose) -ne 0) { Exit }

