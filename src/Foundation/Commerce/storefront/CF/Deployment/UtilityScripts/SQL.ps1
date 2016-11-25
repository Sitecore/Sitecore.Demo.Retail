function Add-SQLPSSnapin
{
	#
	# Add the SQL Server Provider.
	#

	$ErrorActionPreference = "Stop";

    $shellIds = Get-ChildItem HKLM:\SOFTWARE\Microsoft\PowerShell\1\ShellIds;

    if(Test-Path -Path "HKLM:\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.SqlServer.Management.PowerShell.sqlps") {
        $sqlpsreg = "HKLM:\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.SqlServer.Management.PowerShell.sqlps"
    }
    elseif(Test-Path -Path "HKLM:\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.SqlServer.Management.PowerShell.sqlps110") { 
        try{
			if((Get-PSSnapin -Registered |? { $_.Name -ieq "SqlServerCmdletSnapin110"}).Count -eq 0) {

				Write-Host "Registering the SQL Server 2012 Powershell Snapin";

				if(Test-Path -Path $env:windir\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe) {
					Set-Alias installutil $env:windir\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe;
				} 
				elseif (Test-Path -Path $env:windir\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe) {
					Set-Alias installutil $env:windir\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe;
				}
				else {
					throw "InstallUtil wasn't found!";
				}

				if(Test-Path -Path "$env:ProgramFiles\Microsoft SQL Server\110\Tools\PowerShell\Modules\SQLPS\") {
					installutil "$env:ProgramFiles\Microsoft SQL Server\110\Tools\PowerShell\Modules\SQLPS\Microsoft.SqlServer.Management.PSProvider.dll";
					installutil "$env:ProgramFiles\Microsoft SQL Server\110\Tools\PowerShell\Modules\SQLPS\Microsoft.SqlServer.Management.PSSnapins.dll";
				}
				elseif(Test-Path -Path "${env:ProgramFiles(x86)}\Microsoft SQL Server\110\Tools\PowerShell\Modules\SQLPS\") {
					installutil "${env:ProgramFiles(x86)}\Microsoft SQL Server\110\Tools\PowerShell\Modules\SQLPS\Microsoft.SqlServer.Management.PSProvider.dll";
					installutil "${env:ProgramFiles(x86)}\Microsoft SQL Server\110\Tools\PowerShell\Modules\SQLPS\Microsoft.SqlServer.Management.PSSnapins.dll"; 
				}
                    
				Add-PSSnapin SQLServer*110;
				Write-Host "Sql Server 2012 Powershell Snapin registered successfully.";
			} 
		}catch{}

        $sqlpsreg = "HKLM:\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.SqlServer.Management.PowerShell.sqlps110";
    }
    elseif(Test-Path -Path "HKLM:\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.SqlServer.Management.PowerShell.sqlps120") { 
        try{
			if((Get-PSSnapin -Registered |? { $_.Name -ieq "SqlServerCmdletSnapin120"}).Count -eq 0) {

				Write-Host "Registering the SQL Server 2014 Powershell Snapin";

				if(Test-Path -Path $env:windir\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe) {
					Set-Alias installutil $env:windir\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe;
				} 
				elseif (Test-Path -Path $env:windir\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe) {
					Set-Alias installutil $env:windir\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe;
				}
				else {
					throw "InstallUtil wasn't found!";
				}

				if(Test-Path -Path "$env:ProgramFiles\Microsoft SQL Server\120\Tools\PowerShell\Modules\SQLPS\") {
					installutil "$env:ProgramFiles\Microsoft SQL Server\120\Tools\PowerShell\Modules\SQLPS\Microsoft.SqlServer.Management.PSProvider.dll";
					installutil "$env:ProgramFiles\Microsoft SQL Server\120\Tools\PowerShell\Modules\SQLPS\Microsoft.SqlServer.Management.PSSnapins.dll";
				}
				elseif(Test-Path -Path "${env:ProgramFiles(x86)}\Microsoft SQL Server\120\Tools\PowerShell\Modules\SQLPS\") {
					installutil "${env:ProgramFiles(x86)}\Microsoft SQL Server\120\Tools\PowerShell\Modules\SQLPS\Microsoft.SqlServer.Management.PSProvider.dll";
					installutil "${env:ProgramFiles(x86)}\Microsoft SQL Server\120\Tools\PowerShell\Modules\SQLPS\Microsoft.SqlServer.Management.PSSnapins.dll"; 
				}
                    
				Add-PSSnapin SQLServer*120;
				Write-Host "Sql Server 2014 Powershell Snapin registered successfully.";
			} 
		}catch{}

        $sqlpsreg = "HKLM:\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.SqlServer.Management.PowerShell.sqlps120";
    }
	elseif(Test-Path -Path "HKLM:\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.SqlServer.Management.PowerShell.sqlps130") { 
        try{
			if((Get-PSSnapin -Registered |? { $_.Name -ieq "SqlServerCmdletSnapin130"}).Count -eq 0) {

				Write-Host "Registering the SQL Server 2016 Powershell Snapin";

				if(Test-Path -Path $env:windir\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe) {
					Set-Alias installutil $env:windir\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe;
				} 
				elseif (Test-Path -Path $env:windir\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe) {
					Set-Alias installutil $env:windir\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe;
				}
				else {
					throw "InstallUtil wasn't found!";
				}

				if(Test-Path -Path "$env:ProgramFiles\Microsoft SQL Server\130\Tools\PowerShell\Modules\SQLPS\") {
					installutil "$env:ProgramFiles\Microsoft SQL Server\130\Tools\PowerShell\Modules\SQLPS\Microsoft.SqlServer.Management.PSProvider.dll";
					installutil "$env:ProgramFiles\Microsoft SQL Server\130\Tools\PowerShell\Modules\SQLPS\Microsoft.SqlServer.Management.PSSnapins.dll";
				}
				elseif(Test-Path -Path "${env:ProgramFiles(x86)}\Microsoft SQL Server\130\Tools\PowerShell\Modules\SQLPS\") {
					installutil "${env:ProgramFiles(x86)}\Microsoft SQL Server\130\Tools\PowerShell\Modules\SQLPS\Microsoft.SqlServer.Management.PSProvider.dll";
					installutil "${env:ProgramFiles(x86)}\Microsoft SQL Server\130\Tools\PowerShell\Modules\SQLPS\Microsoft.SqlServer.Management.PSSnapins.dll"; 
				}
                    
				Add-PSSnapin SQLServer*130;
				Write-Host "Sql Server 2016 Powershell Snapin registered successfully.";
			} 
		}catch{}

        $sqlpsreg = "HKLM:\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.SqlServer.Management.PowerShell.sqlps130";
    }
    else {
        throw "SQL Server Provider for Windows PowerShell is not installed."
    }

    $item = Get-ItemProperty $sqlpsreg
	$sqlpsPath = [System.IO.Path]::GetDirectoryName($item.Path)

	#
	# Set mandatory variables for the SQL Server provider
	#
	Set-Variable -scope Global -name SqlServerMaximumChildItems -Value 0
	Set-Variable -scope Global -name SqlServerConnectionTimeout -Value 30
	Set-Variable -scope Global -name SqlServerIncludeSystemObjects -Value $false
	Set-Variable -scope Global -name SqlServerMaximumTabCompletion -Value 1000

	#
	# Load the snapins, type data, format data
	#
	Push-Location
	
    cd $sqlpsPath 
    	
    if (Get-PSSnapin -Registered | where {$_.name -eq 'SqlServerProviderSnapin100'}) 
    { 
        if( !(Get-PSSnapin | where {$_.name -eq 'SqlServerProviderSnapin100'})) 
        {
            Add-PSSnapin SqlServerProviderSnapin100; 
        }  
        
        if( !(Get-PSSnapin | where {$_.name -eq 'SqlServerCmdletSnapin100'})) 
        {
            Add-PSSnapin SqlServerCmdletSnapin100;
        }
        
        Write-Host "Using the SQL Server 2008 Powershell Snapin.";
          
       Update-TypeData -PrependPath SQLProvider.Types.ps1xml -ErrorAction SilentlyContinue
       Update-FormatData -prependpath SQLProvider.Format.ps1xml -ErrorAction SilentlyContinue
    } 
    else #Sql Server 2012 or 2014 module should be registered now.  Note, we'll only use it if the earlier version isn't installed.
    { 
        Write-Host "Using the SQL Server 2012 or 2014 Powershell Module.";

        if( !(Get-Module | where {$_.name -eq 'sqlps'})) 
        {  
            Import-Module 'sqlps' -DisableNameChecking; 
        }	
        cd $sqlpsPath;
        cd ..\PowerShell\Modules\SQLPS;
        Update-TypeData -PrependPath SQLProvider.Types.ps1xml -ErrorAction SilentlyContinue
        Update-FormatData -prependpath SQLProvider.Format.ps1xml -ErrorAction SilentlyContinue
	}
    
    Pop-Location
}

function Add-SQL-Database
{
	param
	(
		[String]$dbName=$(throw 'Parameter -dbName is missing!'),
		[String]$mdf_file=$(throw 'Parameter -mdf_file is missing!'),
		[String]$ldf_file=$(throw 'Parameter -ldf_file is missing!')
	)

	Write-Host "Adding database $dbName from files $mdf_file and $ldf_file" -ForegroundColor Green
	Invoke-Sqlcmd -Query "CREATE DATABASE $dbName ON (FILENAME = '$($mdf_file)'), (FILENAME = '$($ldf_file)') FOR ATTACH;"
}


function Drop-SQL-Database
{
	param
	(
		[String]$dbName=$(throw 'Parameter -dbName is missing!')
	)
	
	try
	{
		$server = new-object ("Microsoft.SqlServer.Management.Smo.Server")
        if($server.Databases.Contains($dbName))
        {
            Write-Host "Attemping to delete database $dbName" -ForegroundColor Green -NoNewline
		    Invoke-Sqlcmd -Query "ALTER DATABASE [$($dbName)] SET OFFLINE WITH ROLLBACK IMMEDIATE"
		    Invoke-Sqlcmd -Query "DROP DATABASE [$($dbName)]"
		    Write-Host "    DELETED" -ForegroundColor DarkGreen
        }
        else
        {
            Write-Warning "$dbName does not exist, cannot delete"
        }
	}
	catch
	{
		Write-Host "    Unable to delete database $dbName" -ForegroundColor Red
	}
}

function Add-SQL-User-to-Role
{
	param
	(
		[String]$dbName=$(throw 'Parameter -dbName is missing!'),
        [String]$userName=$(throw 'Parameter -userName is missing!'),
        [String]$role=$(throw 'Parameter -role is missing!')
	)
    Write-Host "Attempting to add the user $userName to database $dbName as role $role" -ForegroundColor Green -NoNewline

    try
    {
        Invoke-Sqlcmd -Query "IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '$($userName)') BEGIN USE [$($dbName)] CREATE USER [$($userName)] FOR LOGIN [$($userName)] END"
        Invoke-Sqlcmd -Query "USE [$($dbName)] EXEC sp_addrolemember '$($role)', '$($userName)'"
        Write-Host "     Added" -ForegroundColor DarkGreen
    }
    catch
    {
        Write-Host ""
        Write-Host "Error: Unable to add user $userName`nDetails: $_" -ForegroundColor Red
    }
}