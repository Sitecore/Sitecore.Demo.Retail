# Arguments
#======================================
# Release #
param(
    [string]$csfReleaseNum = $(throw 'Please specify the Storefront revision used: e.g. 8.1.207.0'),
    [string]$vNextReleaseNum = $(throw 'Please specify the VNect revision used: e.g. 20151030.3'),
	[string]$vNextReleaseType = "CI",
    [string]$deploy = "Deploy-CopyFilesOnly"
)

. .\BaseFunctions.ps1
LoadEnvironmentXml -ConfigurationIdentity "Domain.Dev.Unpup"

function Log
{
	param(
		[string]$level = "h1",
		[string]$msg = ""
	)
	
	switch ($level)
	{
		"h1" 
		{ 
			Write-Host ""
			Write-Host "========================================" -ForegroundColor Green ;
			Write-Host "|| $msg" -ForegroundColor Green ;
			Write-Host "========================================" -ForegroundColor Green ;
		}
		"err" 
		{ 
			Write-Host $msg -ForegroundColor Red ;
		}
		default 
		{
			Write-Host $msg -ForegroundColor White ;
		}
	}
}

# Common Variables
#======================================
$INSTALL_PATH = "C:\CommerceServices"
$LOG_FILE = "$INSTALL_PATH\Deploy-CopyPackages.log"

$RELEASE_BUILD_PATH_CSF = "\\fil1ca2\builds01\csx\CSX_CSF_REL"
$RELEASE_BUILD_PATH_VNEXT = "\\fil1ca2\BUILDS01\Commerce.Engine.$vNextReleaseType"

$INSTALL_PATH_PACKAGES = "$INSTALL_PATH\Packages"
$INSTALL_PATH_ZIPS = "$INSTALL_PATH\Zips"

$RELEASE_PATH_ZIP = "..\..\..\Deployment\Install\zips\latest"
$RELEASE_PATH_PACKAGES = "..\..\..\Deployment\Install\packages\latest"
$RELEASE_PATH_CSF = "$RELEASE_BUILD_PATH_CSF\$csfReleaseNum"
$RELEASE_PATH_VNEXT = "$RELEASE_BUILD_PATH_VNEXT\Commerce.Engine.$vNextReleaseType" + "_" + "$vNextReleaseNum\drop"

function Create-Directory-Structure
{
	Log -level h1 -msg "Create-Directory-Structure"
	# Create directory structure
	#======================================
	# C:\CommerceServices
	#                \Packages
    #                \Zips
	New-Item -ItemType directory -Path $INSTALL_PATH_PACKAGES
    New-Item -ItemType directory -Path $INSTALL_PATH_ZIPS
}

function Copy-Release-Files
{
	Log -level "h1" -msg "Copy-Release-Files"
	
	Copy-Item "$RELEASE_PATH_ZIP\Sitecore*.zip" -destination "$INSTALL_PATH_ZIPS" -force
	Copy-Item "$RELEASE_PATH_ZIP\Adventure*.zip" -destination "$INSTALL_PATH_ZIPS" -force
	Copy-Item "$RELEASE_PATH_PACKAGES\Sitecore Commerce Server Connect*.update" -destination "$INSTALL_PATH_PACKAGES" -force

    Copy-Item "$RELEASE_PATH_CSF\Packages" -destination $INSTALL_PATH -recurse -container -force
	Copy-Item "$RELEASE_PATH_CSF\DeployCS\Database\*" -destination "$DATABASE_DIRECTORY" -recurse -force
   
    Copy-Item "$RELEASE_PATH_VNEXT\Sitecore.Commerce.Engine.Connect*.update" -destination $INSTALL_PATH_PACKAGES -recurse -container -force
    Copy-Item "$RELEASE_PATH_VNEXT\Sitecore.Commerce.Engine*.zip" -destination $INSTALL_PATH_ZIPS -recurse -container -force
	Copy-Item "$RELEASE_PATH_VNEXT\CommerceServicesDbScript.sql" -destination $INSTALL_PATH_ZIPS -recurse -container -force
}

function Deploy-CopyFilesOnly
{
    Log -level "h1" -msg "Deploy-CopyFilesOnly"

	try
	{
		if(Test-Path $INSTALL_PATH)
		{
			Write-Host "Attemping to delete site directory $INSTALL_PATH"
			Remove-Item $INSTALL_PATH -Recurse -Force
			Write-Host "$INSTALL_PATH deleted" -ForegroundColor Green
		}
		else
		{
			Write-Warning "$INSTALL_PATH does not exist, no need to delete"
		}
	}
	catch
	{
		Write-Error "Failed to delete $INSTALL_PATH"
	}

	Create-Directory-Structure
	Copy-Release-Files
}

# Main
#==========================================================
# IGNORE_TRANSCRIPT is used to indicate to other called scripts, e.g. Deploy.ps1,
# to ignore starting/stopping of their own transcription, such that there will only
# be a single log created.
$IGNORE_TRANSCRIPT = ""

Start-Transcript -path $LOG_FILE -append

if (Get-Command $deploy -errorAction SilentlyContinue)
{
    . $deploy
}
else
{
	Log -level "err" -msg "Unknown deployment command: $deploy"
}
Stop-Transcript