$ErrorActionPreference = "Stop";
Set-PSDebug -Strict

Write-Host "Loading CS2007 Marketing Assemblies";
[Void][System.Reflection.Assembly]::Load("CommerceServer.Core.Internal.Marketing, Version=10.0.0.0, Culture=neutral, PublicKeyToken=f5c79cb11734af7a");
[Void][System.Reflection.Assembly]::Load("CommerceServer.Core.Runtime.Configuration, Version=10.0.0.0, Culture=neutral, PublicKeyToken=f5c79cb11734af7a");

##
#.Synopsis
#  Import an exported Marketing Pup
#.Parameter CommerceSiteName
#  The Commerce Server site name
#.Parameter XmlPath
#  The directory to the location of the exported Marketing data
##
function CS2007-ImportMarketingData
{
	PARAM
	(
		[String]$CSSiteName=$(throw 'Parameter -CommerceSiteName is missing!'),
		[String]$XmlPath=$(throw 'Parameter -XmlPath is missing!')
    )
	Trap
	{
		Write-Host "Error: $($_.Exception.GetType().FullName)" -ForegroundColor Red ; 
		Write-Host $_.Exception.Message; 
		Write-Host $_.Exception.StackTrack;
		break;
	}

       # let Pup validates the site so we get a descriptive error if something is wrong
	   if( Test-Path -Path $XmlPath)
	   {
	   		Write-Host "The specified input directory $($XmlPath) does not exist";
	   }

   		Write-Host "Starting Import";
       $marketingPup = new-object CommerceServer.Core.Internal.Marketing.MarketingPup();
       $marketingPup.Import($CSSiteName, "Marketing", $XmlPath, 0);

   		Write-Host "Marketing Import Complete.";
   }
}