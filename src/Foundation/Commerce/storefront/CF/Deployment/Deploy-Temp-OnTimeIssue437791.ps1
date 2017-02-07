# Workaround for Package install failure in Sitecore 8 Update 3 / Sitecore 8.1 rev. 150523

#Here is a workaround to avoid this problem:

#1. In Web.config file, configuration/configSections node, add following line:
#<section name="sitecorediff" type="Sitecore.Update.Configuration.ConfigReader, Sitecore.Update"/>
#2. After configuration/configSections node, add following line:
#<sitecorediff></sitecorediff>

. .\Deploy-Env-Common.ps1

$SitecoreWebConfigFile = $siteUtilitiesDirDst + "\Web.config"

Write-Host "Modifying web.config with workaround for Update Package Install" -ForegroundColor Green
$doc = New-Object System.Xml.XmlDocument
$doc.Load($SitecoreWebConfigFile)

#add the config section
$configSection = $doc.CreateElement("section");
$configSection.SetAttribute("name", "sitecorediff");
$configSection.SetAttribute("type", "Sitecore.Update.Configuration.ConfigReader, Sitecore.Update");
$doc.configuration.configSections.AppendChild($configSection);

#add the config
$config = $doc.CreateElement("sitecorediff");
$doc.configuration.AppendChild($config);

$doc.Save($SitecoreWebConfigFile);
Write-Host "DONE Modifying web.config" -ForegroundColor Green
