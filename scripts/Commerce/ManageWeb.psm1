function Invoke-WebRequest
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$websiteSettingList,
        [Parameter(Mandatory=$True)][string]$websiteId,
        [Parameter(Mandatory=$True)][string]$relativeUri,
                                    [string]$errorString = $null
    )
    begin {}
    process
    {
        $websiteSetting = ($websiteSettingList | Where { $_.id -eq $websiteId} | Select)
        If ($websiteSetting -eq $null) { Write-Host "Can't derive website '$websiteId' from list of Websites." -ForegroundColor red; return 1; }
        
        $bindingSetting = $websiteSetting.bindings | Select -First 1
        If ($bindingSetting -eq $null) { Write-Host "Website '$($websiteSetting.id)' has no specified binding." -ForegroundColor red; return 1; }
        
        $uri = $bindingSetting.protocol + "://" + $bindingSetting.hostName + ":" + $bindingSetting.port + "/" + $relativeUri
        $response = (Microsoft.PowerShell.Utility\Invoke-webrequest -uri $uri -UseBasicParsing -TimeoutSec 60)
        
        If($response.statuscode -ne 200)
        {
            Write-Host "Webrequest failed to '$uri' with status code $($response.statuscode)" -foregroundcolor red
            Write-Host "Webrequest content: $($response.content)" -foregroundcolor red
            return 1;
        }
        ElseIf (($errorString -ne "") -and ($response.content -like $errorString))
        {
            Write-Host "Error string '$errorString' detected in web response from '$uri' with status code $($response.statuscode)" -foregroundcolor red
            Write-Host "Webrequest content: $($response.content)" -foregroundcolor red
            return 1;
        }

        Write-verbose "Webrequest successful to '$uri' with status code $($response.statuscode)"
        return 0;
    }
    end { }
}

Export-ModuleMember Invoke-WebRequest