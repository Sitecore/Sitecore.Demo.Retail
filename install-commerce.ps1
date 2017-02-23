Set-ExecutionPolicy Unrestricted –scope CurrentUser
if (-Not ($PSVersionTable.PSVersion.Major -ge 4)) { Write-Host "Update version of powershell"; exit }

#If Install-Module is not available - install https://www.microsoft.com/en-us/download/details.aspx?id=51451
if((Get-Module Carbon) -eq 0 ) { Install-Module -Name 'Carbon'; Import-Module 'Carbon' }
Import-Module $PSScriptRoot\Scripts\Commerce\ManageUser.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\ManageFile.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\ManageCommerceServer.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\ManageIIS.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\ManageRegistry.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\ManageSqlServer.psm1 -Force
cd $PSScriptRoot

If ((ManageUser\Confirm-Admin) -ne $true) { Write-Host "Please run script as administrator"; exit }
If ((ManageRegistry\Get-InternetExplorerEnhancedSecurityEnabled) -eq $true) { Write-Host "Please disable Internet Explorer Enhanced Security"; exit }

$settings = ((Get-Content $PSScriptRoot\install-commerce-config.json -Raw) | ConvertFrom-Json)
$runTimeUserSetting = ($settings.accounts | Where { $_.id -eq "runTime" } | Select)

$installFolderSetting = ($settings.resources | Where { $_.id -eq "install" } | Select)
If ($installFolderSetting -eq $null) { Write-Host "Expected install resources" -ForegroundColor red; exit; }
  
$dacpacFolderSetting = ($settings.resources | Where { $_.id -eq "dacpac" } | Select)
If ($dacpacFolderSetting -eq $null) { Write-Host "Expected dacpac resources" -ForegroundColor red; exit; }   

$csResourceFolderSetting = ($settings.resources | Where { $_.id -eq "commerceServerResources" } | Select)
If ($csResourceFolderSetting -eq $null) { Write-Host "Expected dacpac resources" -ForegroundColor red; exit; }         

# Step 0: check if all files exist that are needed
Write-Host "`nStep 0: Checking if all needed files exist" -foregroundcolor Yellow
if ((ManageFile\Confirm-Resources $settings.resources -verbose) -ne 0) { Exit }

# Step 1: Create Required Users
Write-Host "`nStep 1: Create Required Users" -foregroundcolor Yellow
If((ManageUser\Add-User -user $runTimeUserSetting -Verbose) -ne 0) { Exit }

#step 2: Create Database Logins
Write-Host "`nStep 2: Create Databse Logins" -foregroundcolor Yellow
If((ManageSqlServer\New-SqlLogin -accountId "runTime" -databaseId "commerceAdminDB" -accountSettingList $settings.accounts -databaseSettingList $settings.databases -Verbose) -ne 0) { Exit }

# Step 3: Run Commerce Server Installer
Write-Host "`nStep 3: Run Commerce Server Installer" -foregroundcolor Yellow
If((ManageCommerceServer\Install-CS -installFolderSetting $installFolderSetting -Verbose) -ne 0) { Exit }

Import-Module CSPS -Force

# Step 4: Run Commerce Server Configuration
Write-Host "`nStep 4: Run Commerce Server Configuration" -foregroundcolor Yellow
If((ManageCommerceServer\Enable-CS -path $installFolderSetting.path -csConfigSetting $settings.sitecoreCommerce.csInstallerConfig -databaseSettingList $settings.databases -accountSettingList $settings.accounts -Verbose) -ne 0) { Exit }

# Step 5: Create IIS App Pools
Write-Host "`nStep 5: Create IIS App Pools" -foregroundcolor Yellow
If((ManageIIS\New-AppPool -appPoolSettingList $settings.iis.appPools -accountSettingList $settings.accounts -Verbose) -ne 0) { Exit }

# Step 6: Create IIS Websites
Write-Host "`nStep 6: Create IIS Websites" -foregroundcolor Yellow
If((ManageIIS\New-Website -websiteSettingList $settings.iis.websites -appPoolSettingList $settings.iis.appPools -Verbose) -ne 0) { Exit }

# Step 7: Configure host file
Write-Host "`nStep 7: Configure Host File" -foregroundcolor Yellow
If((ManageIIS\Set-HostFile -hostEntryList $settings.iis.hostEntries -Verbose) -ne 0) { Exit }

# Step 8: Create Commerce Server Site
Write-Host "`nStep 8: Create Commerce Server Site" -foregroundcolor Yellow
$result = (ManageCommerceServer\New-CSWebsite -csSiteSetting $settings.sitecoreCommerce.csSite -accountSettingList $settings.accounts -appPoolSettingList $settings.iis.appPools -websiteSettingList $settings.iis.websites -installFolderSetting $installFolderSetting -databaseSettingList $settings.databases -Verbose)
If($result -ne 0) { Exit }

# Step 9: Publish Database Changes
Write-Host "`nStep 9: Publish Database Changes" -foregroundcolor Yellow
If((ManageSqlServer\Publish-DatabaseChanges -dacpacFolderSetting $dacpacFolderSetting -databaseSettingList $settings.databases -Verbose) -ne 0) { Exit }

# Step 10: Publish Commerce Server Resources
Write-Host "`nStep 10: Publish Commerce Server Resources" -foregroundcolor Yellow
If((ManageCommerceServer\Import-CSSiteData -csSiteSetting $settings.sitecoreCommerce.csSite -csResourceFolderSetting $csResourceFolderSetting -Verbose) -ne 0) { Exit }

# Step 11: Disable-LoopbackCheck (for development machines)
Write-Host "`nStep 11: Disable-LoopbackCheck (for development machines)" -foregroundcolor Yellow
ManageRegistry\Disable-LoopbackCheck

# Step 11: Test Commerce Server WebServices
#Write-Host "`nStep 11: Test Commerce Server WebServices" -foregroundcolor Yellow
#TODO
#testWebServices -services $settings.CS.webservices -username $settings.CS.user.csusername -pwd $settings.CS.user.password -domain "http://localhost" -port $settings.CS.site.port -sitename $settings.CS.site.sitename -Verbose

# Step 12: Creat self signed certificate
