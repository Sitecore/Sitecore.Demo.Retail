#Requires -Version 3
# Deploy-SC-CS.ps1
.\Deploy-DeployCommerceServices.ps1
.\Deploy-BootstrapCommerceServices.ps1
.\Deploy-Mongo-Clean.ps1
.\Deploy-Base-SC.ps1
.\Deploy-CopyFiles.ps1
.\Deploy-InstallModules.ps1
.\Deploy-InstallPackages.ps1
.\Deploy-UpdateConfigs.ps1
.\Deploy-Base-SC-Merge.ps1
.\Deploy-InitializeCommerceServices.ps1
.\Deploy-PublishToWeb.ps1
.\Deploy-RebuildIndices.ps1