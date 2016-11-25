#Requires -Version 3
. ( Join-Path -Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent) -ChildPath "Deploy.ps1" );

#check for MVC4 installation
if(!(Test-Path "${Env:ProgramFiles(x86)}\Microsoft ASP.NET\ASP.NET MVC 4"))
{
    Write-Error "ASP.NET MVC 4 is required for this installation, please install from http://www.asp.net/downloads"
    return
}

Deploy-CommerceServer -ConfigurationIdentity "Domain.Dev.SC" -PreCompile;

# launch Sitecore
if($launchSitecore)
{
	$url = "http://" + $scHostHeaderName + "/sitecore/login"
	Write-Host "Launching new sitecore instance at $url" -ForegroundColor Green
    Start-Sleep -Milliseconds 500
	start $url
}

# done
Write-Host "Done setting up Sitecore Instance $instanceName" -ForegroundColor Green
