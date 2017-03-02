Import-Module PKI
Import-Module WebAdministration

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

function Test-WebService
{
    param 
    (
        [Parameter(Mandatory=$True)][string]$uri,
        [Parameter(Mandatory=$True)][string]$username,
        [Parameter(Mandatory=$True)][string]$pwd
    )
    begin {}
    process
    {
        [hashtable]$return = @{}
        #$req = [system.Net.WebRequest]::Create($uri)
        $passwd = ConvertTo-SecureString $pwd -AsPlainText -Force
        $credentials = New-Object System.Management.Automation.PSCredential ($username, $passwd);
        $req = New-WebServiceProxy -Uri $uri  -Credential $credentials
        
        try
        {
            $res = $req.GetServiceVersion()
        }
        catch [System.Net.WebException]
        {
            $res = $_.Exception.Response
        }
        return $res;
    }
    end { }
}

function New-Certificate
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$certificateSettingList
    )
    begin 
    {
        Write-Verbose "Setting Up Certificates"
    }
    process
    {
        Foreach ($certificateSetting in $certificateSettingList)
        {
            Write-Verbose "Setting up Certificate for: $($certificateSetting.DnsName)"

            $certificate = Get-ChildItem $certificateSetting.certStoreLocation | where-object { $_.DnsNameList -like $certificateSetting.dnsName }  | Select-Object -First 1

            If(-not $certificate)
            {
                PKI\New-SelfSignedCertificate -DnsName $certificateSetting.dnsName -CertStoreLocation $certificateSetting.certStosreLocation -verbose | Write-Verbose
                $certificate = Get-ChildItem $certificateSetting.certStoreLocation | where-object { $_.Subject -like $certificateSetting.dnsName }  | Select-Object -First 1
      
                If(-not $certificate)
                {
                    Write-Host "Error, unable to create certificate for $($certificateSetting.name)" -ForegroundColor red
                    return 1; 
                }
            }
New-WebBinding -Name TestWebsite -Protocol https -Port 443 -HostHeader TestsslBinding -IPAddress 192.168.1.108 -SslFlags 1
$session = New-PsSession –ComputerName '.'
Invoke-Command -Session $session {get-item -Path "cert:\LocalMachine\My\$($certificateSetting.name)" | new-item -path "IIS:\SslBindings\0.0.0.0!443"}
<#
calhost -Query "Select * from SSLBinding"

foreach ($obj in $objWmi)
{
	write-host "BindingOwnerID:" $obj.BindingOwnerID
	write-host "CertificateCheckMode:" $obj.CertificateCheckMode
	write-host "CertificateHash:" $obj.CertificateHash
	write-host "CertificateStoreName:" $obj.CertificateStoreName
	write-host "CTLIdentifier:" $obj.CTLIdentifier
	write-host "CTLStoreName:" $obj.CTLStoreName
	write-host "IPAddress:" $obj.IPAddress
	write-host "Port:" $obj.Port
	write-host "RevocationFreshnessTime:" $obj.RevocationFreshnessTime
	write-host "RevocationURLRetrievalTimeout:" $obj.RevocationURLRetrievalTimeout
	write-host "SslAlwaysNegoClientCert:" $obj.SslAlwaysNegoClientCert
	write-host "SslUseDsMapper:" $obj.SslUseDsMapper
	write-host
	write-host "########"
	write-host
}


$site1 = Get-Website | Where {$_.Name -eq $certificateSetting.name}
$site1Bindings = $site1 | Get-WebBinding




            Get-ChildItem IIS:\SslBindings | ? write-host

            if (-not (Get-ChildItem IIS:\SslBindings | ?{ $_.host -eq "*.example.com" -and $_.port -eq 443 }))
            {
            }

            Write-Host "tesT"
            #Push-Location IIS:\SslBindings
            #Get-Item cert:\LocalMachine\My\$certificate.Thumbprint | New-Item 0.0.0.0!443;
            #Pop-Location
            #>
<#
# test binding, create if missing
# fails on wildcard test when the binding exists
if (-not (Test-Path -LiteralPath IIS:\SslBindings\*!443!*.example.com))
{ 
    Push-Location Cert:\LocalMachine\My

    # find or create a certificate
    $targetCert = Get-ChildItem -Recurse | ? { ($_.NotAfter -gt (Get-Date)) -and ($_.DnsNameList -contains "*.example.com") } | Sort NotAfter -Descending | select -First 1
    if ($targetCert -eq $null)
    {
        $targetCert = New-SelfSignedCertificate -DnsName "*.example.com" -CertStoreLocation Cert:\LocalMachine\My
    }

    # bind to host header *
    $targetCert | New-Item -Path IIS:\SslBindings\*!443!*.example.com -SSLFlags 1

    Pop-Location
}

            # Go to the PowerShell Drive for IIS 
            Push-Location IIS:\SslBindings
 
            # Bind the HTTPS protocol to the Default Website (listening on all IP addresses)
            New-WebBinding -Name $certificateSetting.name -IP $certificateSetting.ip -Port $certificateSetting.port -Protocol $certificateSetting.protocol
 
            # Look at the binding collection using the following command: 
            Get-WebBinding $certificateSetting.name 
 
            # Bind the Self-Signed certificate to the WebBinding
            $strThumb = $SelfSignedCert.Thumbprint
            Get-Item Cert:\LocalMachine\MY\$strThumb | New-Item "$certificateSetting.ip!$certificateSetting.port"
            Pop-Location
        #>
        }

        return 0;
    }
    end { }  
}

Export-ModuleMember New-AppPool, New-Website, Set-HostFile, Remove-Site, Remove-AppPool, Test-WebService, New-Certificate