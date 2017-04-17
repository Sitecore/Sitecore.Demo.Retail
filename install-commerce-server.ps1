Set-ExecutionPolicy Unrestricted –scope CurrentUser
if (-Not ($PSVersionTable.PSVersion.Major -ge 4)) { Write-Host "Update version of powershell"; exit }

#If Install-Module is not available - install https://www.microsoft.com/en-us/download/details.aspx?id=51451
if(-Not (Get-Module -ListAvailable -Name Carbon)) { Install-Module -Name Carbon -AllowClobber; Import-Module Carbon}

Import-Module $PSScriptRoot\Scripts\Commerce\ManageUser.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\ManageFile.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\ManageCommerceServer.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\ManageRegistry.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\ManageIIS.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\ManageSqlServer.psm1 -Force
Import-Module $PSScriptRoot\Scripts\Commerce\UserRights.ps1 -Force
cd $PSScriptRoot

If ((ManageUser\Confirm-Admin) -ne $true) { Write-Host "Please run script as administrator" -ForegroundColor red; exit }
If ((ManageRegistry\Get-InternetExplorerEnhancedSecurityEnabled -Verbose) -eq $true) { Write-Host "Please disable Internet Explorer Enhanced Security" -ForegroundColor red; exit }
If ((ManageRegistry\Get-WindowsIdentityFoundationEnabled -Verbose) -eq $true) { Write-Host "Please install Windows Feature - Windows Identity Foundation" -foregroundcolor red; Exit }

$settings = ((Get-Content $PSScriptRoot\install-commerce-config.json -Raw) | ConvertFrom-Json)
$runTimeUserSetting = ($settings.accounts | Where { $_.id -eq "runTime" } | Select)

$installFolderSetting = ($settings.resources | Where { $_.id -eq "install" } | Select)
If ($installFolderSetting -eq $null) { Write-Host "Expected install resources" -ForegroundColor red; exit; }
  
$csResourceFolderSetting = ($settings.resources | Where { $_.id -eq "commerceServerResources" } | Select)
If ($csResourceFolderSetting -eq $null) { Write-Host "Expected dacpac resources" -ForegroundColor red; exit; }         

# Step 0: check if all files exist that are needed
Write-Host "`nStep 0: Checking if all needed files exist" -foregroundcolor Yellow
if ((ManageFile\Confirm-Resources $settings.resources -verbose) -ne 0) { Exit }

# Step 1: Create Required Users
Write-Host "`nStep 1: Create Required Users" -foregroundcolor Yellow
If((ManageUser\Add-User -user $runTimeUserSetting -Verbose) -ne 0) { Exit }

# Step 2: Create Database Logins
Write-Host "`nStep 2: Create Database Logins" -foregroundcolor Yellow
If((ManageSqlServer\New-SqlLogin -accountId "runTime" -databaseId "commerceAdminDB" -accountSettingList $settings.accounts -databaseSettingList $settings.databases -Verbose) -ne 0) { Exit }

# Step 3: Enable Windows Authentication
Write-Host "`nStep 3: Enable Windows Authentication" -foregroundcolor Yellow
If((ManageIIS\Enable-WindowsAuthentication -Verbose) -ne 0) { Exit }

# Step 4: Run Commerce Server Installer
Write-Host "`nStep 4: Run Commerce Server Installer" -foregroundcolor Yellow
If((ManageCommerceServer\Install-CS -installFolderSetting $installFolderSetting -Verbose) -ne 0) { Exit }

# Step 5: Run Commerce Server Configuration
Write-Host "`nStep 5: Run Commerce Server Configuration" -foregroundcolor Yellow
If((ManageCommerceServer\Enable-CS -path $installFolderSetting.path -csConfigSetting $settings.sitecoreCommerce.csInstallerConfig -databaseSettingList $settings.databases -accountSettingList $settings.accounts -Verbose) -ne 0) { Exit }