#Requires -Version 3
#
#
# This script will modify the location element by commenting out the httpModules section
#
#

trap
{
	Write-Host "Error: $($_.Exception.GetType().FullName)" -ForegroundColor Red ; 
	Write-Host $_.Exception.Message; 
	Write-Host $_.Exception.StackTrack;
	return;
}

# Change the instance name to the Sitecore instance name you want to delete
$instanceName = "CFRefStorefront"

$installDir = "c:\inetpub\" + $instanceName
$SitecoreWebsiteFolder = $installDir + "\Website"

Write-Host "Verifying web.config for httpModules registrations"

$doc = New-Object System.Xml.XmlDocument
$doc.Load($SitecoreWebsiteFolder + "\Web.config")
$node = $doc.SelectSingleNode("//configuration/location/system.web/httpModules")
if(!$node -eq "" -and !$node.InnerXml.Contains("<!-- "))
{
    # Commenting httpModules section in web.config
    Write-Host "Making backup of web.config"
    $webconfigFile = Get-Item($SitecoreWebsiteFolder + "\web.config")
    Copy-Item $webconfigFile ($SitecoreWebsiteFolder + "\web.config.bak") 

    Write-Host "Modifying web.config - Commenting location/system.web/httpModules"
    $node.InnerXml = "<!-- " + $node.InnerXml + " -->"
    $doc.Save($SitecoreWebsiteFolder + "\web.config")
}
else
{
    Write-Host "Web.config did not contain any httpModules registrations or they where already commented"
}
