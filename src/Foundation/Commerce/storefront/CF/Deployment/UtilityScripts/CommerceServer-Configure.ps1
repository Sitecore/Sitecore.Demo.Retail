#Requires -Version 2
$ErrorActionPreference = "Stop";
Set-PSDebug -Strict;

Write-Host "Loading .NET Assemblies";
[Void][System.Reflection.Assembly]::Load("ADODB, Version=7.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
[Void][System.Reflection.Assembly]::Load("System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

################################################################
#.Synopsis
#  Configure an instance of Commerce Server 2007
################################################################
function CS2007-Configure()
{
	PARAM
	(
		[String]$DatabaseServer=$(throw 'Parameter -DatabaseServer is missing!'),
		[String]$StagingServiceUserDomain=$(throw 'Parameter -StagingServiceUserDomain is missing!'),
		[String]$StagingServiceUsername=$(throw 'Parameter -StagingServiceUsername is missing!'),
		[String]$StagingServiceUserPassword=$(throw 'Parameter -StagingServiceUserPassword is missing!'),
		[String]$MailerServiceUserDomain=$(throw 'Parameter -MailerServiceUserDomain is missing!'),
		[String]$MailerServiceUsername=$(throw 'Parameter -MailerServiceUsername is missing!'),
		[String]$MailerServiceUserPassword=$(throw 'Parameter -MailerServiceUserPassword is missing!')
    )

	Write-Host "Configuring Commerce Server 2007/2009";

	#remove the read-only attribute
	$path = (Join-Path -Path $DEPLOYMENT_DIRECTORY -ChildPath "csconfig.xml");
	if (Clear-FileAttribute $path "ReadOnly" -ne $true )
	{
		Write-Error -Message "Cannot clear Read-Only flag for file [$($path)].";
	}

    # Get the content of the config file and cast it to XML and save a backup copy labeled .bak followed by the date
	$xml = [xml](get-content $path);
	$xml.Save($path + "." + (Get-Date –f yyyyMMddHHmmss) + ".bak");
	
	$root = $xml.get_DocumentElement();
	
	#Update the staging service identity
	$root.SelectSingleNode("NTService[@ID='StagingService']").UserName = $StagingServiceUsername;
	$root.SelectSingleNode("NTService[@ID='StagingService']").Domain = $StagingServiceUserDomain;
	$root.SelectSingleNode("NTService[@ID='StagingService']").Password = $StagingServiceUserPassword;
	
	#Update the direct mailer service identity
	$root.SelectSingleNode("NTService[@ID='DirectMailerService']").UserName = $MailerServiceUsername;
	$root.SelectSingleNode("NTService[@ID='DirectMailerService']").Domain = $MailerServiceUserDomain;
	$root.SelectSingleNode("NTService[@ID='DirectMailerService']").Password = $MailerServiceUserPassword;
	
	#Update the database connection strings
	$root.SelectSingleNode("SQL[@ID='CommerceAdminDB']").Server = $DatabaseServer;
	$root.SelectSingleNode("SQL[@ID='DirectMailerDB']").Server = $DatabaseServer;
	
	# Save it
	$xml.Save($path);

    $returnValue = Start-Process -FilePath ( Join-Path -Path $Env:COMMERCE_SERVER_ROOT -ChildPath "csconfig.exe") -ArgumentList @( "/s $($path)", "/l $($DEPLOYMENT_DIRECTORY)\csconfig.log", "/f" ) -NoNewWindow -Wait -PassThru;
	$logContent = Get-Content -Path  (Join-Path -Path $DEPLOYMENT_DIRECTORY -ChildPath "csconfig.log") | Out-String;
	Write-Host $logContent;

	if( $returnValue.ExitCode -ne 0 )
	{
		Write-Error "Program Exit Code was $($returnValue.ExitCode), aborting.`r$($returnValue.StandardError)";
	}
}

################################################################
#.Synopsis
#  Unpup the Commerce Server web services
################################################################
function CS2007-UnpupWebServices()
{
	PARAM
	(
		[String]$CSSiteName=$(throw 'Parameter -CommerceSiteName is missing!'),
		[String]$PupFileLocation=$(throw 'Parameter -PupFileLocation is missing!'),
		[String]$PupIniTemplate=$(throw 'Parameter -pupIniTemplate is missing!')
    )
	
	Write-Host "Unpupping CS Web Services";

	$pupIniPath = ( Join-Path -Path $DATABASE_DIRECTORY -ChildPath "\pup\PupConfig.ini" );
	$pupIniContent = Get-Content -Path $($PupIniTemplate);
	$pupIni = $pupIniContent | foreach { $ExecutionContext.InvokeCommand.ExpandString( $_ ) };
	
	Out-File -FilePath $pupIniPath -Force -InputObject $pupIni;
	
	$returnValue = Start-Process -FilePath "$($Env:COMMERCE_SERVER_ROOT)\pup.exe" -ArgumentList @("/u", " /s:$($CSSiteName)", "/f:$($PupFileLocation)", "/i:$($pupIniPath)") -NoNewWindow -Wait -PassThru;
    
	#if( $returnValue.ExitCode -ne 0 )
	#{
	#	Write-Warning "Program Exit Code was $($returnValue.ExitCode), aborting.`r$($returnValue.StandardError).  Make sure you have 'IIS Metabase and IIS 6 Configuration Compatibility' feature installed on IIS7+ machines (via Programs and Features/Windows Features or search for 'Turn Windows Features On and Off').  Also, be sure to delete your existing Profiles database, and Profiles resource via Commerce Server Manager, or the process will fail because the resource already exists.";
	#}
}

function Update-CatalogAuthStore()
{
	PARAM
	(
		[String]$CSSiteName=$(throw 'Parameter -CSSiteName is missing!'),
		[String]$AzManFile=$(throw 'Parameter -AzManFile is missing!')
	)
	
	Write-Host "Updating AzMan store for $($AzManFile)" -ForegroundColor Green;
	$returnValue = Start-Process -FilePath "$($Env:COMMERCE_SERVER_ROOT)\tools\CreateCatalogAuthorizationStore.exe" -ArgumentList @("$CSSiteName", "$AzManFile") -NoNewWindow -Wait -PassThru;
	
	if( $returnValue.ExitCode -ne 0 )
	{
		Write-Error "Program Exit Code was $($returnValue.ExitCode), aborting.`r$($returnValue.StandardError)";
	}	
	
}