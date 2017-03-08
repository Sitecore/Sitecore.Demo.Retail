function Restore-SolutionPackages
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$resourcesSettingList,
        [Parameter(Mandatory=$True)][string]$resourceId
    )
    begin {}
    process
    {
        $resourceSetting = ($resourcesSettingList | Where { $_.id -eq $resourceId} | Select)
        If ($resourceSetting -eq $null) { Write-Host "Can't derive resource '$resourceId' from list of Resources." -ForegroundColor red; return 1; }

        cd $resourceSetting.path
        & dotnet restore | Write-Verbose

        return 0;
    }
    end { }
}

function Publish-Project
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$resourcesSettingList,
        [Parameter(Mandatory=$True)][PSCustomObject]$websiteSettingList,
        [Parameter(Mandatory=$True)][string]$sourceResourceId,
        [Parameter(Mandatory=$True)][string]$targetWebsiteId
    )
    begin {}
    process
    {
        $sourceResourceSetting = ($resourcesSettingList | Where { $_.id -eq $sourceResourceId} | Select)
        If ($sourceResourceSetting -eq $null) { Write-Host "Can't derive resource '$sourceResourceId' from list of Resources." -ForegroundColor red; return 1; }

        $targetWebsiteSetting = ($websiteSettingList | Where { $_.id -eq $targetWebsiteId} | Select)
        If ($targetWebsiteSetting -eq $null) { Write-Host "Can't derive website '$targetWebsiteId' from list of Websites." -ForegroundColor red; return 1; }

        & dotnet publish $sourceResourceSetting.path -o $targetWebsiteSetting.physicalPath  | Write-Verbose

        return 0;
    }
    end { }
}

Export-ModuleMember Restore-SolutionPackages, Publish-Project