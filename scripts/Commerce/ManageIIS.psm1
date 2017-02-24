function New-AppPool
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$appPoolSettingList,
        [Parameter(Mandatory=$True)][PSCustomObject]$accountSettingList
    )

    begin 
    {
        Write-Verbose "Creating Application Pools"
    }
    process
    {
        Foreach ($appPool in $appPoolSettingList)
        {
            if (Test-IisAppPool -Name $appPool.name)
            {
                Write-Verbose "App Pool already exists: $($appPool.id)"
            }
            else 
            {
                Write-Verbose "Creating App Pool: $($appPool.id)"

                $account = ($accountSettingList | Where { $_.id -eq $appPool.accountId } | Select)
                If ($account -eq $null)
                {
                    Write-Host "Account '$($appPool.accountId)' can't be found in list of supplied accounts when processing application pool '$($appPool.id)'." -ForegroundColor red
                    return 1;
                }

                $secpasswd = ConvertTo-SecureString $account.password -AsPlainText -Force
                $appPoolCredential = New-Object System.Management.Automation.PSCredential ($account.username, $secpasswd)
                Install-IisAppPool -Name $appPool.name -ManagedRuntimeVersion $appPool.managedRuntimeVersion -Credential $appPoolCredential
            }
        }

        return 0;
    }
    end{}
}

function New-Website
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$websiteSettingList,
        [Parameter(Mandatory=$True)][PSCustomObject]$appPoolSettingList
    )

    begin 
    {
        Write-Verbose "Creating Websites"
    }
    process
    {
        Foreach ($website in $websiteSettingList)
        {
            if (Test-IisWebsite -Name $website.siteName)
            {
                Write-Verbose "Website already exists: $($website.id)"
            }
            else 
            {
                Write-Verbose "Creating Website: $($website.id)"

                $appPool = ($appPoolSettingList | Where { $_.id -eq $website.appPoolId } | Select)
                If ($appPool -eq $null)
                {
                    Write-Host "App Pool '$($website.appPoolId)' can't be found in list of supplied appPools. Expected for website '$($website.id)'" -ForegroundColor red
                    return 1;
                }

                $bindingList = @()
                Foreach ($binding in $website.bindings)
                {
                    $bindingList += ,"$($binding.protocol)/$($binding.ipAddress):$($binding.port):$($binding.hostName)"
                }

                Install-IisWebsite -Name $website.siteName -PhysicalPath $website.physicalPath -AppPoolName $appPool.name -Bindings $bindingList
            }
        }

        return 0;
    }
    end
    {
        Write-Verbose "Created Websites"
    }
}

function Set-HostFile
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$hostEntryList
    )

    begin 
    {
        Write-Verbose "Setting Up Host File"
    }
    process
    {
        Foreach ($hostEntry in $hostEntryList)
        {
            Write-Verbose "Setting Host Entry: $($hostEntry.hostName)"

            Set-HostsEntry -IPAddress $hostEntry.ipAddress -HostName $hostEntry.hostName -Description $hostEntry.description
        }

        return 0;
    }
    end
    {
        Write-Verbose "Setting Up Host File Completed"
    }
}

function Remove-Site
{
    param 
    (
        [Parameter(Mandatory=$True)][string]$name
    )

    begin {}
    process
    {
        if (Test-IisWebsite -Name $name)
        {
            $site = Get-IisApplication -SiteName $name
            ForEach ($path in $site.PhysicalPath)
            {
                if (Test-Path $path)
                {
                    Remove-Item -Recurse -Force $path
                }
            }
            Uninstall-IisWebsite -Name $name    
            Write-Verbose "Removed IISSite $name"
        }
        else 
        {
            Write-Verbose "IISSite $name does not exist" 
        }
    }
    end {}
}

function Remove-AppPool
{
    param 
    (
        [Parameter(Mandatory=$True)][string]$name
    )

    begin {}
    process
    {
        if (Test-IisAppPool -Name $name)
        {
            Uninstall-IisAppPool -Name $name
            Write-Verbose "Removed appPool $name"
        }
        else 
        {
            Write-Verbose "appPool $name does not exist"   
        }
    }
    end {}
}

Export-ModuleMember New-AppPool, New-Website, Set-HostFile, Remove-Site, Remove-AppPool