import-module sqlps -DisableNameChecking 3>$Nul


"Loading 'Microsoft.SqlServer.Smo' assembly..."
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.Smo") | Out-Null
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SmoExtended") | Out-Null
[Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.ConnectionInfo") | Out-Null
[Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SmoEnum") | Out-Null

Function Detach-Database ($DbName, $ServerName, [switch]$force)
{
<#
.SYNOPSIS
Detaches database from local SQl server.

.DESCRIPTION
Function performs detaching DB with defined name on local SQL server.

.PARAMETER Name
Specifies database name.

.PARAMETER ServerName
Specifies SQL server name.

.EXAMPLE
Detach-Database "Test_Sitecore.Core" "."
#>

  $Server = New-Object Microsoft.SqlServer.Management.Smo.Server -ArgumentList $ServerName

  if ($Server.Databases.Contains($DbName))
  {
    Write-Verbose "Set database offline $DbName" 
	
    $iter = 0
    $successful = 0
    do
    {
	  $iter += 1

	  Write-Verbose "Detaching database $DbName on server $ServerName, attempt: $iter, total: 5" 
      try
      {
        if($force)
        {
          $Server.KillDatabase($DbName)
        }
        else
        {
          $MyDatabase = $Server.Databases.Item($DbName)
          $Server.KillAllprocesses($DbName)
          $MyDatabase.SetOffline()
          $Server.DetachDatabase($DbName, $false) | Out-Null
        }
        $successful = 1
      }
      catch
      {
        Write-Verbose "Detaching database $DbName on server $ServerName, attempt failed"  
        $successful = 0
      }
      if (($successful -eq 0) -and ($iter -gt 4))
      {
        throw "Failed to detach $DbName on server $ServerName"
      }
    }
    until (($successful -ne 0) -or ($iter -gt 4))
  }
  else
  {
    Write-Verbose "Database $DbName was not found on server $ServerName. Nothing performed"  
  }

  if ($Server.Databases.Contains($DbName))
  {
    Write-Verbose "Database $DbName has not been successfully detached ..." 
  }
  else
  {
    Write-Verbose "Database $DbName has been successfully detached..." 
  }
}

Function Detach-Databases ($Prefix, $ServerName, [switch]$force)
{
<#
.SYNOPSIS
Detaches databases from local SQl server.

.DESCRIPTION
Function performs detaching DBs starting with prefix on local SQL server.

.PARAMETER Prefix
Specifies database prefix.

.PARAMETER ServerName
Specifies SQL server name.

.EXAMPLE
Detach-Databases "Test" "."
#>

  Write-Verbose "Detaching databases with prefix $Prefix on server $ServerName" 
  $Server = New-Object Microsoft.SqlServer.Management.Smo.Server -ArgumentList $ServerName
  Write-Verbose "Searching databases with prefix $Prefix" 
  $dbs = $Server.Databases | Where-Object { $_.Name -Like $Prefix + "_*" } | select Name
  if (($dbs -eq $null) -or ($dbs -eq ""))
  {
    Write-Verbose "Databases with prefix $Prefix was not found on server $ServerName"  
  }
  else
  {
    foreach ($db in $dbs)
    {
      Detach-Database -DbName $db.Name -ServerName $ServerName -force:$force
    }
  }
}

Function Attach-Database($dbPath, $dbName, $prefix, $serverName)
{
<#
.SYNOPSIS
Attaches database to local SQL server.

.DESCRIPTION
Function performs attaching DB with defined name on local SQL server.

.PARAMETER db
Specifies database name.

.PARAMETER ServerName
Specifies SQL server name.

.EXAMPLE
Attach-Database "@{FullName=C:\inetpub\wwwroot\TestDeploy\Databases\Sitecore.Master.MDF; DBName=TestDeploy_Sitecore.Master; LogFullName=C:\inetpub\wwwroot\TestDeploy\Databases\Sitecore.Master.ldf}" "."
#>

  $Server = New-Object Microsoft.SqlServer.Management.Smo.Server -ArgumentList $ServerName
  $sc = new-object System.Collections.Specialized.StringCollection;
  if ($prefix.length -gt 0) {
  $db = gci $dbpath -filter $dbname* | Where-Object {$_.Extension -like '*.mdf'}  | Select-Object FullName,@{Name = "DBName"; Expression = {$prefix + "_" + $_.BaseName}},@{Name = "LogFullName"; Expression = {$_.Directory.ToString() + "\" + $_.BaseName + ".ldf"}}
  }
  else {
  $db = gci $dbpath -filter $dbname* | Where-Object {$_.Extension -like '*.mdf'}  | Select-Object FullName,@{Name = "DBName"; Expression = {$_.BaseName}},@{Name = "LogFullName"; Expression = {$_.Directory.ToString() + "\" + $_.BaseName + ".ldf"}}
  }
  $sc.Add($db.FullName) | Out-Null;
  $sc.Add($db.LogFullName) | Out-Null;
  if ($Server.Databases.Contains($db.DBName))
  {
    Write-Verbose "Database $db.DBName is already attached. Detaching database on server $ServerName" 
    Detach-Database $db.DBName $ServerName
  }
  
  Write-Verbose "Attaching database $db String Collection: $sc  on server $ServerName" 
  $Srv = New-Object Microsoft.SqlServer.Management.Smo.Server -ArgumentList $ServerName
  $Srv.ConnectionContext.StatementTimeout = 0
  
  $Srv.AttachDatabase($db.DBName, $sc) | Out-Null
  Write-Verbose "Database has been attached $db.DBName locating at $db.FullName with log file $db.LogFullName on server $ServerName" 
}

Function Attach-Databases ($DbPath, $Prefix, $ServerName)
{
<#
.SYNOPSIS
Attaches databases from defined folder to local SQL server.

.DESCRIPTION
Function performs attaching DBs with defined prefix from defined folder on local SQL server.

.PARAMETER DbPath
Specifies path to databases.

.PARAMETER Prefix
Specifies database prefix.

.PARAMETER ServerName
Specifies SQL server name.

.EXAMPLE
Attach-Databases "C:\Databases" "Test" "."
#>

  Push-Location $DbPath
  $dbs = [string[]](Get-ChildItem *.mdf | select -Expand Name)
  if($dbs -eq $null)
  {
    Write-Verbose "Databases with prefix $Prefix was not found on path $DbPath" 
  }
  else
  {
    foreach ($db in $dbs)
    {
      Attach-Database -DbPath $DbPath -dbName $db -prefix $Prefix -serverName $serverName
    }
  }
  Pop-Location
}


function Import-DatabaseChanges
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$databaseSettingList,
        [Parameter(Mandatory=$True)][PSCustomObject]$resourcesSettingList,
        [Parameter(Mandatory=$True)][PSCustomObject]$resourceFolderId
    )

    begin 
    {
        Write-Verbose "Importing database files from resourceFolderId: $resourceFolderId"
    }
    process
    {
        $folderSetting = ($resourcesSettingList | Where { $_.id -eq $resourceFolderId } | Select)
        If ($folderSetting -eq $null) { Write-Host "Expected $resourceFolderId resources" -ForegroundColor red; return 1; }   

        Foreach ($fileSetting in $folderSetting.files)
        {
            $databaseSetting = ($databaseSettingList | Where { $_.id -eq $fileSetting.databaseId })
            If ($databaseSetting -eq $null) 
            { 
                Write-Host "Can't derive database from DatabaseId '$($fileSetting.databaseId)' when processing file '$($fileSetting.name)'." -ForegroundColor red; 
                return 1; 
            }         

            $databaseName = If ([string]::IsNullOrEmpty($fileSetting.useDatabaseName) -or ($fileSetting.useDatabaseName -ne $false)) { $databaseSetting.name }
            $filename = $folderSetting.path + "\" + $fileSetting.fileName
            
            If($filename -like '*.dacpac') 
            {
                If((Import-Dacpac -filePath $filename -destinationDatabase $databaseName -server $databaseSetting.server -Verbose) -ne 0 ) { return 1 }
            }
            ElseIf ($filename -like '*.sql')
            {
                If((Import-SQL -filePath $filename -destinationDatabase $databaseName -serverinstance $databaseSetting.server -Verbose) -ne 0 ) { return 1 }
            }
            Else
            {
                Write-Host "Unsupported file type '$($fileSetting.name)'." -ForegroundColor red; 
                return 1; 
            }
        }

        return 0
    }
    end{}
}

function Import-SQL
{

    param (
        [Parameter(Mandatory=$True)][string]$filePath,
                                    [string]$destinationDatabase,
        [Parameter(Mandatory=$True)][string]$serverinstance="."
    )

    begin 
    {
        Write-Verbose "Importing '$filePath' to database '$destinationDatabase'"
    }
    process
    {
        If ([string]::IsNullOrEmpty($destinationDatabase))
        {
            $test = Invoke-Sqlcmd -inputfile $filePath -serverinstance $serverinstance -Verbose
            Invoke-Sqlcmd -inputfile $filePath -serverinstance $serverinstance -Verbose
        }
        Else
        {
            Invoke-Sqlcmd -inputfile $filePath -serverinstance $serverinstance -database $destinationDatabase -Verbose
        }

        return 0;
    }
    end{}
}

function Import-Dacpac
{
    param 
    (
        [Parameter(Mandatory=$True)][string]$filePath,
        [Parameter(Mandatory=$True)][string]$destinationDatabase,
        [Parameter(Mandatory=$True)][string]$server
    )

    begin 
    {
        Write-Verbose "Importing Dacpac '$filePath' to database '$destinationDatabase' on server '$server'."
    }
    process
    {
        # could not find a reliable way to find the location of Microsoft.SqlServer.Dac.dll
        $sqlDACFolder="C:\Program Files (x86)\Microsoft SQL Server\140\DAC\bin\Microsoft.SqlServer.Dac.dll" 
        if ((Test-Path ($sqlDACFolder)) -eq $false) 
        {
            $sqlDACFolder="C:\Program Files (x86)\Microsoft SQL Server\130\DAC\bin\Microsoft.SqlServer.Dac.dll"
        }
		if ((Test-Path ($sqlDACFolder)) -eq $false) 
        {
            $sqlDACFolder="C:\Program Files (x86)\Microsoft SQL Server\120\DAC\bin\Microsoft.SqlServer.Dac.dll"
        }
        if ((Test-Path ($sqlDACFolder)) -eq $false) 
        {
            $sqlDACFolder="C:\Program Files (x86)\Microsoft SQL Server\110\DAC\bin\Microsoft.SqlServer.Dac.dll"
        }
        if ((Test-Path ($sqlDACFolder)) -eq $false) 
        { 
            Write-Host "Can't find Microsoft.SqlServer.Dac.dll." -ForegroundColor red; 
            return 1; 
        }

        Add-Type -path $sqlDACFolder
        $dacService = new-object Microsoft.SqlServer.Dac.DacServices "server=$server"
        $dp = [Microsoft.SqlServer.Dac.DacPackage]::Load($filePath)
        $dacService.deploy($dp, $destinationDatabase, "True") 

        return 0;
    }
    end{}
}

function Remove-Database
{
    param 
    (
        [Parameter(Mandatory=$True)][string]$dbname,
        [string]$SQLInstanceName="."
    )

    begin {}
    process
    {
        [System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SMO") | out-null
        $Server = New-Object -TypeName Microsoft.SqlServer.Management.Smo.Server -ArgumentList $SQLInstanceName
        #create SMO handle to your database
        $DBObject = $Server.Databases[$dbname]
        
        #check database exists on server
        if ($DBObject)
        {
                #instead of drop we will use KillDatabase
                #KillDatabase drops all active connections before dropping the database.
                $Server.KillDatabase($dbname)
                Write-Verbose "Deleted Database: $dbname "
        }
        else 
        {
            $servername = $Server.Name
            Write-Verbose "Database: $dbname does not exist in server $servername" 
        }
    }
    end {}
}

function Get-SqlLogin 
{ 
    param 
    ( 
        [Parameter(Mandatory=$True)][string]$serverName, 
        [Parameter(Mandatory=$True)][string]$username 
    ) 
    begin 
    { 
        [System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.SMO') | Out-Null 
    } 
    process 
    { 
        try 
        { 
            $server = new-object ('Microsoft.SqlServer.Management.Smo.Server') $serverName
            $login = $server.Logins | where { $_.Name -eq $username }
              
            return $login 
        }
        catch 
        { 
            Write-Error "Error: $($_.Exception.Message) - Line Number: $($_.InvocationInfo.ScriptLineNumber)" 
        } 
    } 
}

function Get-SqlLoginName
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$accountSetting
    )

    begin {}
    process
    {
        $domain = $accountSetting.domain
        if ($domain -eq ".")
        {
            $d = Get-WmiObject Win32_ComputerSystem
            $domain = $d.PSComputerName
        }
        return ($domain + "\" + $accountSetting.username)
    }
    end {}
}

function New-SqlLogin
{
    param 
    (
        [Parameter(Mandatory=$True)][string]$accountId,
        [Parameter(Mandatory=$True)][string]$databaseId,
        [Parameter(Mandatory=$True)][PSCustomObject]$accountSettingList,
        [Parameter(Mandatory=$True)][PSCustomObject]$databaseSettingList
    )

    begin {}
    process
    {
        $accountSetting = ($accountSettingList | Where { $_.id -eq $accountId } | Select)
        If ($accountSetting -eq $null) { Write-Host "Can't derive account from AccountId '$accountId'." -ForegroundColor red; return 1; }

        $databaseSetting = ($databaseSettingList | Where { $_.id -eq $databaseId } | Select)
        If ($databaseSetting -eq $null) { Write-Host "Can't derive database from DatbaseId '$databaseId'." -ForegroundColor red; return 1; }

        $loginname = Get-SqlLoginName -accountSetting $accountSetting

        if (-not (Get-SqlLogin -serverName $databaseSetting.server -username $loginname))
        {
            $query = "CREATE LOGIN [$loginname] FROM WINDOWS WITH DEFAULT_DATABASE=[master]; ALTER SERVER ROLE [$($accountSetting.defaultSqlServerRole)] ADD MEMBER [$loginname];"
            Invoke-Sqlcmd -ServerInstance $databaseSetting.server -Database "master" -Query $query
            Write-Verbose "SQL login created '$loginname' with role: $role"
        }
        else
        {
            Write-Verbose "SQL user '$loginname' already exists" 
        }

        return 0;
    }
    end {}
}

function Remove-SqlLogin
{
    param 
    (
        [Parameter(Mandatory=$True)][string]$accountId,
        [Parameter(Mandatory=$True)][string]$databaseId,
        [Parameter(Mandatory=$True)][PSCustomObject]$accountSettingList,
        [Parameter(Mandatory=$True)][PSCustomObject]$databaseSettingList
    )

    begin {}
    process
    {
        $accountSetting = ($accountSettingList | Where { $_.id -eq $accountId } | Select)
        If ($accountSetting -eq $null) { Write-Host "Can't derive account from AccountId '$accountId'." -ForegroundColor red; return 1; }

        $databaseSetting = ($databaseSettingList | Where { $_.id -eq $databaseId } | Select)
        If ($databaseSetting -eq $null) { Write-Host "Can't derive database from DatbaseId '$databaseId'." -ForegroundColor red; return 1; }

        [string]$loginname = (Get-SqlLoginName -accountSetting $accountSetting)

        if (Get-SqlLogin -serverName $databaseSetting.server -username $loginname)
        {
            $query = "DROP LOGIN [$loginname]"
            Invoke-Sqlcmd -ServerInstance $databaseSetting.server -Database "master" -Query $query
            Write-Verbose "Deleted SQL login '$loginname'"
        }
        else 
        {
            Write-Verbose "SQL user '$loginname' does not exist" 
        }
    }
    end {}
}

Export-ModuleMember Import-DatabaseChanges, Import-Dacpac, Remove-Database, New-SqlLogin, Remove-SqlLogin, Attach-Databases, Detach-Databases