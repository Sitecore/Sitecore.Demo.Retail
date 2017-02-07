# Performs a deployment of the Sitecore Commerce Storefront for Sitecore 8.2

#==========================================================
# Environment Setup
#==========================================================
$PS_ROOT=$env:SystemDrive
$CONFIG_ROOT="$PS_ROOT\SitecorePowerShell\DeploymentConfigs"
$DEPLOYMENT_ROOT="$CONFIG_ROOT\DeploymentTemplates"

#==========================================================
# Set Deployment file
#==========================================================
$c = ".\Sitecore82.deployment.json"  

# Convert config/deployment files to a PSCustomObject and resolve to display object values
#$o = Get-ConfigObject $c -edit
#$o | Out-GridView

# Deploy Sitecore only
# Check if your C:\00Deployments\SiteAAA contains the content you want to publish
# If the folder does not exists or doesn't contain the proper version, uncomment this line and comment the Prerequesites section.
Install-Deployment $c -runActionGroup sfcs-Grp-Setup

# Otherwise, you have all the source packages and modules in place and you just need to setup the prerequisite
# Prerequesites for the site deployment
#Install-Deployment $c -runActionFromLibrary sc-PrepareSiteFolder
#Install-Deployment $c -runActionFromLibrary sc-CopyLicenseFile
#Install-Deployment $c -runActionFromLibrary sc-CopySiteUtilityFiles
#Install-Deployment $c -runActionFromLibrary sc-GrantUserAccessToSCFolders

# Deployment
Install-Deployment $c -runActionGroup sfcs-Grp-CopyModulesAndPackages
Install-Deployment $c -runActionGroup sc-Grp-ConfigSite
Install-Deployment $c -runActionGroup sfcs-Grp-Install
Install-Deployment $c -runActionGroup sc-Grp-PublishAndRebuildIndices
