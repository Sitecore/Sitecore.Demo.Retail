import-module sqlps -DisableNameChecking 3>$Nul

function Publish-DatabaseChanges
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$dacpacFolderSetting,
        [Parameter(Mandatory=$True)][PSCustomObject]$databaseSettingList
    )

    begin 
    {
        Write-Verbose "Publishing Database Changes"
    }
    process
    {
        Foreach ($dacpacFileSetting in $dacpacFolderSetting.files)
        {
            $databaseSetting = ($databaseSettingList | Where { $_.id -eq $dacpacFileSetting.databaseId })
            If ($databaseSetting -eq $null) 
            { 
                Write-Host "Can't derive database from DatabaseId '$($dacpacFileSetting.databaseId)' when processing dacpac file '$($dacpacFileSetting.name)'." -ForegroundColor red; 
                return 1; 
            }         

            $filename = $dacpacFolderSetting.path + "\" + $dacpacFileSetting.fileName
            
            return (Publish-Dacpac -dacpacFile $filename -destinationDatabase $databaseSetting.name -server $databaseSetting.server -Verbose)
        }

        return 0
    }
    end{}
}

function Publish-Dacpac
{
    param 
    (
        [Parameter(Mandatory=$True)][string]$dacpacFile,
        [Parameter(Mandatory=$True)][string]$destinationDatabase,
        [Parameter(Mandatory=$True)][string]$server
    )

    begin 
    {
        Write-Verbose "Publishing Dacpac '$dacpacFile' to database '$destinationDatabase' on server '$server'."
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
        $dp = [Microsoft.SqlServer.Dac.DacPackage]::Load($dacpacFile)
        $dacService.deploy($dp, $destinationDatabase, "True") 

        return 0
    }
    end{}
}

function Delete-Database
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

function Get-SqlLogin { 
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
            $query = "CREATE LOGIN [$loginname] FROM WINDOWS WITH DEFAULT_DATABASE=[master]; "; #"ALTER SERVER ROLE [$role] ADD MEMBER [$loginname];"
            Invoke-Sqlcmd -ServerInstance $databaseSetting.server -Database "master" -Query $query
            Write-Verbose "SQL login created '$loginname'" # with role: $role"
        }
        else
        {
            Write-Verbose "SQL user '$loginname' allready exists" 
        }

        return 0;
    }
    end {}
}

function Delete-SqlLogin
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

Export-ModuleMember Publish-DatabaseChanges, Publish-Dacpac, Delete-Database, New-SqlLogin, Delete-SqlLogin