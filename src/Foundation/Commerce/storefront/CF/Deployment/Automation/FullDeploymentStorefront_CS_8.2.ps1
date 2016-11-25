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

# Convert config/deployment files to a PSCustomObject and resolve
#$o = Get-ConfigObject $c
# Display object values
#$o = Get-ConfigObject $c -edit
#$o | Out-GridView

# Full deployment of Commerce Server and Sitecore
Install-Deployment $c -runActionGroup sfcs-Grp-FullDeploy
