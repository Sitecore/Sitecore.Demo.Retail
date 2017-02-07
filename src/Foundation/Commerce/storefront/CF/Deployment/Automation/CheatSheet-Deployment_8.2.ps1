#==========================================================
# Environment Setup
#==========================================================
$PS_ROOT='C:'
$CONFIG_ROOT="$PS_ROOT\SitecorePowerShell\DeploymentConfigs"
$DEPLOYMENT_ROOT="$CONFIG_ROOT\DeploymentTemplates"

#==========================================================
# Set Deployment file
#==========================================================
$c = "Sitecore82.deployment.json"  

#==========================================================
# Display/Edit Configs
#==========================================================
$o = Get-ConfigObject $c -edit

#==========================================================
# Deploy
#==========================================================
Install-Deployment $c -runActionGroup sfcs-Grp-FullClean
Install-Deployment $c -runActionGroup sfcs-Grp-FullDeploy

# Setup
#----------------------------------------------------------
Install-Deployment $c -runActionFromLibrary sc-CreateDeploymentDirs
Install-Deployment $c -runActionFromLibrary csCore-CreateTempDir
Install-Deployment $c -runActionFromLibrary csCore-CopyDeploymentFiles
Install-Deployment $c -runActionFromLibrary csCore-CopyDatabase
Install-Deployment $c -runActionFromLibrary sc-CopyDeploymentFiles
Install-Deployment $c -runActionFromLibrary sc-PrepareSiteFolder
Install-Deployment $c -runActionFromLibrary sc-CopyLicenseFile
Install-Deployment $c -runActionFromLibrary sc-CopySiteUtilityFiles
Install-Deployment $c -runActionFromLibrary sc-GrantUserAccessToSCFolders

    # Or use the group 
    Install-Deployment $c -runActionGroup sfcs-Grp-Setup # To reinstall Sitecore or Commerce Server only

# Modules and Packages
Install-Deployment $c -runActionFromLibrary connect-CopyModulesToDeployment
Install-Deployment $c -runActionFromLibrary csConnect-InstallNugetPackages
Install-Deployment $c -runActionFromLibrary csConnect-CopyTools
Install-Deployment $c -runActionFromLibrary csConnect-InstallTools
Install-Deployment $c -runActionFromLibrary csMerch-CopyPackages
Install-Deployment $c -runActionFromLibrary sfCommon-InstallNugetPackages
Install-Deployment $c -runActionFromLibrary sfcs-InstallNugetPackages
Install-Deployment $c -runActionFromLibrary sfcs-InstallNugetPackageAdvWorksImages

    # Or use the group 
    Install-Deployment $c -runActionGroup sfcs-Grp-CopyModulesAndPackages # To reinstall Sitecore only

# CS Install and Config
#----------------------------------------------------------
Install-Deployment $c -runActionFromLibrary csCore-CreateUsers
Install-Deployment $c -runActionFromLibrary csCore-InstallCsExe
Install-Deployment $c -runActionFromLibrary csCore-GrantUserAccessToCSFolders
Install-Deployment $c -runActionFromLibrary csCore-UpdateRegistry
Install-Deployment $c -runActionFromLibrary csCore-NewAppPools
Install-Deployment $c -runActionFromLibrary csCore-NewSites
Install-Deployment $c -runActionFromLibrary csCore-Setup
Install-Deployment $c -runActionFromLibrary csCore-ImportData
Install-Deployment $c -runActionFromLibrary csCore-SetStockHandling

    # Or use the group 
    Install-Deployment $c -runActionGroup csCore-Grp-InstallAndConfig # To reinstall Commerce Server only

# Sitecore Install and Config
#----------------------------------------------------------
Install-Deployment $c -runActionFromLibrary sc-UpdateConfigs
Install-Deployment $c -runActionFromLibrary sc-CreateDBDirs
Install-Deployment $c -runActionFromLibrary sc-MoveDBs
Install-Deployment $c -runActionFromLibrary sc-AttachDBs
Install-Deployment $c -runActionFromLibrary sc-UpdateConnectionStrings
Install-Deployment $c -runActionFromLibrary sc-NewAppPools
Install-Deployment $c -runActionFromLibrary sc-NewSites
Install-Deployment $c -runActionFromLibrary sc-NewCertificates
Install-Deployment $c -runActionFromLibrary sc-NewBindings
Install-Deployment $c -runActionFromLibrary sc-UpdateRegistry

    # Or use the group 
    Install-Deployment $c -runActionGroup sc-Grp-ConfigSite # To reinstall Sitecore only

    # Install SC with remote DBs
    Install-Deployment $c -runActionGroup sc-Grp-ConfigSiteRemoteDb

# Sitecore Commerce Install and Config
#----------------------------------------------------------
Install-Deployment $c -runActionFromLibrary sccs-RunSqlScripts
Install-Deployment $c -runActionFromLibrary connect-InstallModules
Install-Deployment $c -runActionFromLibrary csConnect-InstallPackages
Install-Deployment $c -runActionFromLibrary csMerch-InstallPackages
Install-Deployment $c -runActionFromLibrary sfCommon-InstallPackages
Install-Deployment $c -runActionFromLibrary sfcs-InstallAdvWorksImages
Install-Deployment $c -runActionFromLibrary sfcs-InstallPackages
Install-Deployment $c -runActionFromLibrary sfcs-UpdateConfigs
Install-Deployment $c -runActionFromLibrary sccs-MergeConfigs

    # Or use the group 
    Install-Deployment $c -runActionGroup sfcs-Grp-Install # To reinstall Sitecore only


Install-Deployment $c -runActionFromLibrary sc-PublishToWeb
Install-Deployment $c -runActionFromLibrary sc-RebuildAllIndices

    # Or use the group 
    Install-Deployment $c -runActionGroup sc-Grp-PublishAndRebuildIndices # To reinstall Sitecore only


# Cleanup
#----------------------------------------------------------
ipmo DeployActionsCS -force
ipmo CsCore -force

Install-Deployment $c -runActionFromLibrary csCore-RemoveSiteArtifacts
Install-Deployment $c -runActionFromLibrary csCore-UninstallCs
Install-Deployment $c -runActionFromLibrary csCore-RemoveCsFolder
Install-Deployment $c -runActionFromLibrary csCore-RemoveSites
Install-Deployment $c -runActionFromLibrary csCore-RemoveAppPools
Install-Deployment $c -runActionFromLibrary csCore-RemoveMSCSAdminDB
Install-Deployment $c -runActionFromLibrary csCore-DropSqlLogins

    # Or use the group cleanup
    Install-Deployment $c -runActionGroup csCore-Grp-Clean

# Sitecore Site 
ipmo SqlServer -force

Install-Deployment $c -runActionFromLibrary sc-DetachDBs
Install-Deployment $c -runActionFromLibrary sc-RemoveSites
Install-Deployment $c -runActionFromLibrary sc-CleanSiteFolder
Install-Deployment $c -runActionFromLibrary sc-RemoveAppPools

    # Or use the group cleanup
    Install-Deployment $c -runActionGroup sc-Grp-Clean  # To clean Sitecore only

