#Requires -Version 2
$ErrorActionPreference = "Stop";
Set-PSDebug -Strict;

. .\CSManager.ps1

Write-Host "Setting the Commerce Server Stock Handling property to enable PreOrderable and BackOrderable products" -ForegroundColor Green;
# Note: This assumes that an iisreset is executed sometime after, since it's required for the changes to take effect
Set-CSConfigInventoryProperty -CommerceSiteName "CFSolutionStorefrontSite" -CommercePropertyName "i_stock_handling" -CommercePropertyValue 1