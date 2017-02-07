#Requires -Version 3
#
#
# use this script only if you have not used the Sitecore EXE installer
#
#

trap
{
	Write-Host "Error: $($_.Exception.GetType().FullName)" -ForegroundColor Red ; 
	Write-Host $_.Exception.Message; 
	Write-Host $_.Exception.StackTrack;
	return;
}


import-module webadministration

$DEPLOYMENT_DIRECTORY=(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent);

. ( Join-Path -Path $DEPLOYMENT_DIRECTORY -ChildPath "UtilityScripts\SQL.ps1");


Add-SQLPSSnapin

Clear-Host

# Change the instance name to the Sitecore instance name you want to delete
$instanceName = "CFRefStorefront"
# Change the reference storefront instance name you want to delete
$hostHeaderName = "cf.reference.storefront.com";

$installDir = "c:\inetpub\" + $instanceName
$SitecoreDataFolder = $installDir + "\Data"
$SitecoreWebsiteFolder = $installDir + "\Website"
$SitecoreDBFolder = $installDir + "\Database"

$Sitecore_DB_Core_Name = $instanceName + "Sitecore_Core"
$Sitecore_DB_Master_Name = $instanceName + "Sitecore_Master"
$Sitecore_DB_Web_Name = $instanceName + "Sitecore_Web"
$Sitecore_DB_DMS_Name = $instanceName + "Sitecore_Analytics"
$Sitecore_DB_Sessions_Name = $instanceName + "Sitecore_Sessions"

$stopIIS = $false

[array]$databases = $Sitecore_DB_Core_Name,$Sitecore_DB_Master_Name,$Sitecore_DB_Web_Name,$Sitecore_DB_DMS_Name,$Sitecore_DB_Sessions_Name
[array]$hostEntries = $instanceName,$hostHeaderName

function remove-host([string]$filename, [string]$hostname) 
{
    $c = Get-Content $filename
	$newLines = @()
	foreach ($line in $c) 
	{
		$bits = [regex]::Split($line, "\t+")
		if ($bits.count -eq 2) 
		{
			if ($bits[1] -ne $hostname) 
			{
				$newLines += $line
			}
            else
            {
                Write-Host "Removing host entry $hostname from $filename"
            }
		} 
		else 
		{
			$newLines += $line
		}
	}
	# Write file
	Clear-Content $filename
	foreach ($line in $newLines) 
	{
		$line | Out-File -encoding ASCII -append $filename
	}
}

#drop databases
foreach($db in $databases)
{
    Drop-SQL-Database $db
}

#remove hosts file entry
foreach($hostname in $hostEntries)
{
    remove-host "C:\WINDOWS\system32\drivers\etc\hosts" $hostname
}

#remove website
if(Test-Path IIS:\Sites\$instanceName)
{
    Write-Host "Removing website $instanceName"
    Invoke-Expression -Command "& $($env:systemroot)\system32\inetsrv\APPCMD.exe delete site `"$instanceName`"";
    $stopIIS = $true
}
else
{
    Write-Warning "Website $instanceName does not exist, no need to delete"
}

if($stopIIS)
{
    Write-Host "Stopping IIS"
    iisreset /stop /timeout:30
}

#delete folder
try
{
	if(Test-Path $installDir)
    {
        Write-Host "Attemping to delete site directory $installDir"
        Remove-Item $installDir -Recurse -Force
        Write-Host "$installDir deleted" -ForegroundColor Green
    }
    else
    {
        Write-Warning "$installDir does not exist, no need to delete"
    }
}
catch
{
	Write-Error "Failed to delete $installDir"
}

if($stopIIS)
{
    Write-Host "Starting IIS"
    iisreset /start
}

Write-Host "Sitecore instance $instanceName removed" -ForegroundColor Green
