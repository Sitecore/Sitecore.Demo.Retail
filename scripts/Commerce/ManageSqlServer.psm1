import-module sqlps -DisableNameChecking 3>$Nul

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

            $databaseName = If ($databaseSetting.useDatabaseName) { $databaseSetting.name } else { $null }
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
            Invoke-Sqlcmd -inputfile $filePath -serverinstance $serverinstance -Verbose 
        }
        Else
        {
            Invoke-Sqlcmd -inputfile $filePath -serverinstance $serverinstance -database $destinationDatabase -Verbose 
        }
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
        $sqlDACFolder="C:\Program Files (x86)\Microsoft SQL Server\130\DAC\bin\Microsoft.SqlServer.Dac.dll" 
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

        return 0
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
            Write-Verbose "SQL user '$loginname' allready exists" 
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

Export-ModuleMember Import-DatabaseChanges, Import-Dacpac, Remove-Database, New-SqlLogin, Remove-SqlLogin