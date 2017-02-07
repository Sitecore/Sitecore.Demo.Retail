#Requires -Version 2
$ErrorActionPreference = "Stop";
Set-PSDebug -Strict;

################################################################
#.Synopsis
#  Used to modify Commerce Server MSCS Admin Inventory Flag 
#  values.
#.Parameter CommerceSiteName
#  The Commerce Server site name
#.Parameter CommercePropertyName
#  The Commerce Server property name to change in the Inventory 
#  configuration.
#.Parameter CommercePropertyValue
#  The CommerceServer configuration property value.
################################################################
function Set-CSConfigInventoryProperty()
{
	PARAM
	(
        [String][Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()]$CommerceSiteName,
        [String][Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()]$CommercePropertyName,
        [Int][Parameter(Mandatory=$true)]$CommercePropertyValue
    )

    # Load Assemblies
    [void][System.Reflection.Assembly]::Load("CommerceServer.Core.Configuration, Version=10.0.0.0, Culture=neutral, PublicKeyToken=f5c79cb11734af7a");
    [void][System.Reflection.Assembly]::Load("ADODB, Version=7.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
     
    # Initialize SiteConfigReadOnlyFreeThreaded            
    Write-Host "Initializing Commerce Server 2007 SiteConfig Object" -ForegroundColor Green;
    $siteConfig = new-object CommerceServer.Core.Configuration.SiteConfig;
    $siteConfig.Initialize($CommerceSiteName);

    $siteConfig.Fields.Item("Inventory").Value.Fields.Item($CommercePropertyName).Value = $CommercePropertyValue;
    $siteConfig.SaveConfig();
    Write-Host "Commerce Server Inventory flag set to '" $($CommercePropertyValue) "' for property '" $($CommercePropertyName) "'" -ForegroundColor Green;
     
    # Release underlying COM resources
    $siteConfig.Dispose();
}
