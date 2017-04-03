function Invoke-WebRequestWithWebsiteId
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
        
        return ManageWeb\Invoke-WebRequest -Uri $uri -errorString $errorString
    }
    end { }
}

function Invoke-WebRequest
{
    param 
    (
        [Parameter(Mandatory=$True)][string]$uri,
                                    [string]$errorString = $null
    )
    begin {}
    process
    {
        Try
        {
            $response = (Microsoft.PowerShell.Utility\Invoke-webrequest -uri $uri -UseBasicParsing -TimeoutSec 360)
        
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
        Catch
        {
            Write-Host "Webrequest failed to '$uri' with Error '$($_.Exception.Message)'" -foregroundcolor red
            return 1;
        }
    }
    end { }
}

function Test-Certificate
{
        param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$certificateSettingList
    )
    begin 
    {
        Write-Verbose "Checking Certificates"
    }
    process
    {
        Foreach ($certificateSetting in $certificateSettingList)
        {
            Write-Verbose "Checking certificate '$($certificateSetting.dnsName)'"
            
            $uri = "https://" + $certificateSetting.dnsName
        
            return Invoke-WebRequest -Uri $uri
        }

        return 0;
    }
    end { }
}

Export-ModuleMember Invoke-WebRequestWithWebsiteId, Invoke-WebRequest, Test-Certificate