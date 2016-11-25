#Requires -Version 3
. .\Deploy-Env-Common.ps1

# Install Packages
Write-Host "Updating configuration..." -ForegroundColor Green; 


# edit Storefront.config to change the host name.
Write-Host "Editing Reference.Storefront.Config file."
$doc = New-Object System.Xml.XmlDocument
$doc.Load($includeDir + "\Reference.Storefront\Reference.Storefront.config")

$node = $doc.SelectSingleNode("//configuration/sitecore/sites/site[@hostName= 'cf.reference.storefront.com']")
if ($node -ne $null)
{
    $node.hostName = $scHostHeaderName
    
    $doc.Save($includeDir + "\Reference.Storefront\Reference.Storefront.config")
}
