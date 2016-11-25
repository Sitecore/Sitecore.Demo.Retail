# Cleans the Sitecore Commerce Storefront deployment for Sitecore 8.2

#==========================================================
# Environment Setup
#==========================================================
$PS_ROOT=$env:SystemDrive
$CONFIG_ROOT="$PS_ROOT\SitecorePowerShell\DeploymentConfigs"
$DEPLOYMENT_ROOT="$CONFIG_ROOT\DeploymentTemplates"

# Load the deployment configuration
$c = '.\Sitecore82.deployment.json' 

# Convert config/deployment files to a PSCustomObject and resolve
#$o = Get-ConfigObject $c -edit
# Display object values
#$o | Out-GridView

# Cleans the storefront installation only
Install-Deployment $c -runActionGroup sc-Grp-Clean
