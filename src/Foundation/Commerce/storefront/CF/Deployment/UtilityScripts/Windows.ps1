function Windows-CreateLocalUser
{
  PARAM
  (
    [String]$UserName=$(throw 'Parameter -UserName is missing!'),
    [String]$Password=$(throw 'Parameter -Password is missing!')
  )
  Trap
  {
    Write-Host "Error: $($_.Exception.GetType().FullName)" -ForegroundColor Red ; 
    Write-Host $_.Exception.Message; 
    Write-Host $_.Exception.StackTrack;
    break;
  }
  
  Write-Host "Creating $($UserName)";
  
  #$response = Invoke-Expression -Command "NET USER $($UserName) `"/add`" $($Password) `"/passwordchg:no`" `"/expires:never`"";
  
  $objOu = [ADSI]"WinNT://$env:COMPUTERNAME";
  $objUser = $objOU.Create("User", $UserName);

  $objUser.setpassword($Password);
  $objUser.SetInfo();

  $objUser.description = "$UserName";
  $objUser.SetInfo();
  
  $objUser.UserFlags.value = $objUser.UserFlags.value -bor 64;
  $objUser.UserFlags.value = $objUser.UserFlags.value -bor 65536;
  $objUser.SetInfo();
  
  
  Write-Host "Response from creating local user: $response";
}

function Windows-CreateLocalGroup
{
  PARAM
  (
    [String]$LocalGroupName=$(throw 'Parameter -GroupName is missing!')
  )
  Trap
  {
    Write-Host "Error: $($_.Exception.GetType().FullName)" -ForegroundColor Red ; 
    Write-Host $_.Exception.Message; 
    Write-Host $_.Exception.StackTrack;
    break;
  }
  
  Write-Host "Adding Localgroup $($LocalGroupName)";
  Invoke-Expression "& NET LOCALGROUP $($LocalGroupName) /add";

}

function Windows-RenameLocalUser
{
  PARAM
  (
    [String]$ComputerName=$(throw 'Parameter -ComputerName is missing!'),
    [String]$OldUserName=$(throw 'Parameter -OldUserName is missing!'),
    [String]$NewUserName=$(throw 'Parameter -NewUserName is missing!')
  )
  Trap
  {
    Write-Host "Error: $($_.Exception.GetType().FullName)" -ForegroundColor Red; 
    Write-Host $_.Exception.Message; 
    Write-Host $_.Exception.StackTrack;
    break;
  }
  $objUser=[adsi]("WinNT://" + $ComputerName + “/$($OldUserName), user”); 
  $objUser.psbase.rename($($NewUserName));
}

function Windows-GrantFullReadWriteAccessToFolder 
{

PARAM
  (
    [String]$Path=$(throw 'Parameter -Path is missing!'),
    [String]$UserName=$(throw 'Parameter -UserName is missing!')
  )
  Trap
  {
    Write-Host "Error: $($_.Exception.GetType().FullName)" -ForegroundColor Red ; 
    Write-Host $_.Exception.Message; 
    Write-Host $_.Exception.StackTrack;
    break;
  }
  
  Windows-GrantAccessToFolder $Path $UserName ([System.Security.AccessControl.FileSystemRights]"FullControl")
}

function Windows-GrantAccessToFolder 
{

PARAM
  (
    [String]$Path=$(throw 'Parameter -Path is missing!'),
    [String]$UserName=$(throw 'Parameter -UserName is missing!'),
    [System.Security.AccessControl.FileSystemRights]$Rights=$(throw 'Parameter -Rights are missing!')
  )
  Trap
  {
    Write-Host "Error: $($_.Exception.GetType().FullName)" -ForegroundColor Red ; 
    Write-Host $_.Exception.Message; 
    Write-Host $_.Exception.StackTrack;
    break;
  }
  
  $InheritanceFlag = [System.Security.AccessControl.InheritanceFlags]::ContainerInherit -bor [System.Security.AccessControl.InheritanceFlags]::ObjectInherit;
  $PropagationFlag = [System.Security.AccessControl.PropagationFlags]::None;
  $objType =[System.Security.AccessControl.AccessControlType]::Allow;
  
  $Acl = (Get-Item $path).GetAccessControl("Access");
  $Ar = New-Object system.security.accesscontrol.filesystemaccessrule("$UserName", $Rights, $InheritanceFlag, $PropagationFlag, $objType);

  for ($i=1; $i -lt 30; $i++)
  {
      try
      {
        Write-Host "Attempt $i to set permissions GrantAccessToFolder"
        $Acl.SetAccessRule($Ar);
        Set-Acl $path $Acl;
        break;
      }
      catch
      {
        Write-Host "Attempt to set permissions failed. Error: $($_.Exception.GetType().FullName)" -ForegroundColor Yellow ; 
        Write-Host $_.Exception.Message; 
        Write-Host $_.Exception.StackTrack;
    
        Write-Host "Retrying command in 10 seconds" -ForegroundColor Yellow ;

        Start-Sleep -Seconds 10
      }
  }
}


function Windows-GrantFullReadWriteAccessToFile 
{

PARAM
  (
    [String]$Path=$(throw 'Parameter -Path is missing!'),
    [String]$UserName=$(throw 'Parameter -UserName is missing!')
  )
  Trap
  {
    Write-Host "Error: $($_.Exception.GetType().FullName)" -ForegroundColor Red ; 
    Write-Host $_.Exception.Message; 
    Write-Host $_.Exception.StackTrack;
    break;
  }
  
  $colRights = [System.Security.AccessControl.FileSystemRights]::ReadAndExecute -bor [System.Security.AccessControl.FileSystemRights]::Modify;
  #$InheritanceFlag = [System.Security.AccessControl.InheritanceFlags]::ContainerInherit -bor [System.Security.AccessControl.InheritanceFlags]::ObjectInherit;
  #$PropagationFlag = [System.Security.AccessControl.PropagationFlags]::None;
  $objType =[System.Security.AccessControl.AccessControlType]::Allow;
  
  $Acl = (Get-Item $Path).GetAccessControl("Access");
  $Ar = New-Object system.security.accesscontrol.filesystemaccessrule($UserName, $colRights, $objType);

  for ($i=1; $i -lt 30; $i++)
  {
      try
      {
        Write-Host "Attempt $i to set permissions GrantFullReadWriteAccessToFile"
        $Acl.SetAccessRule($Ar);
        Set-Acl $path $Acl;
        break;
      }
      catch
      {
        Write-Host "Attempt to set permissions failed. Error: $($_.Exception.GetType().FullName)" -ForegroundColor Yellow ; 
        Write-Host $_.Exception.Message; 
        Write-Host $_.Exception.StackTrack;
    
        Write-Host "Retrying command in 10 seconds" -ForegroundColor Yellow ;

        Start-Sleep -Seconds 10
      }
  }
}

function Windows-CreateDirectory
{
  PARAM
  (
    [String]$DirectoryName=$(throw 'Parameter -DirectoryName is missing!'),
    [String]$Path=$(throw 'Parameter -DirectoryName is missing!')
  )
  Trap
  {
    Write-Host "Error: $($_.Exception.GetType().FullName)" -ForegroundColor Red ; 
    Write-Host $_.Exception.Message; 
    Write-Host $_.Exception.StackTrack;
    break;
  }
  $FullPath=Join-Path -Path $($Path) -ChildPath $($DirectoryName);
  
  if ( Test-Path -Path $FullPath)
  {
    Write-Host "Directory already exists";
    throw;
  }
  else
  {
    Invoke-Expression "& mkdir  $FullPath";
  }
}

function Windows-CreateShortCut
{
    PARAM
    (
        $Path = $(throw "Name parameter missing."),
        $TargetPath = $(throw "TargetPath parameter missing."),
        $Arguments = "",
        $WorkingDirectory = "",
        $Description = ""
    )
    
    $wshshell = New-Object -ComObject WScript.Shell;
    $link = $wshshell.CreateShortcut($Path);
    $link.TargetPath = $TargetPath;
    $link.Arguments = $Arguments;
    $link.WorkingDirectory = $WorkingDirectory;
    $link.Description = $Description;
    $link.Save();
}

function Windows-UpdateHostsFile
(
  [string]$IPAddress,
  [string]$Hostname
)
{
    [string]$hostsPath = "c:\windows\system32\drivers\etc\hosts";
    $lines = ( Get-Content -Path $hostsPath ) ;

    if($lines)
    {
        $uncommentedLines = $lines | where { ( $_.StartsWith("#") -ne $true ) };
 
        if($uncommentedLines)
        {
            $lineToFind = "$($IPAddress)`t$($Hostname)";

            if( $uncommentedLines | where { $_ -match "$($lineToFind)" } )
            {
                return;
            }
        }
    }   
    
    Add-Content -Path $hostsPath -Value "`r`n$($IPAddress)`t$($Hostname)"; 
    (gc $hostsPath) | ? {$_.trim() -ne "" } | set-content $hostsPath
}

function Windows-GetUserNameFromLogin
(
  [string]$LoginName
)
{
   return $LoginName.Substring($LoginName.IndexOf("\") + 1)
}

function Windows-GetDomainFromLogin
(
  [string]$LoginName
)
{
   return $LoginName.Substring(0,$LoginName.IndexOf("\")) 
}

<#
.Synopsis
  Add a domain user to a local windows group on any machine.
.Description
.Parameter $ComputerName,
.Example
  PS> Windows-AddUserToLocalGroup -ComputerName $ComputerName -Account 'YourAccount'
.Link
  about_functions
  about_functions_advanced
  about_functions_advanced_methods
  about_functions_advanced_parameters
 .Notes
  Uses WMI to talk to the local/remote machine.   
#>
function Windows-AddUserToLocalGroup
{
  param (
    [Parameter(Position=0, ValueFromPipeline=$true)]$ComputerName = $Env:COMPUTERNAME,
    [Parameter(Position=1, Mandatory=$true)]$UserName,
    [Parameter(Position=2, Mandatory=$true)]$LocalGroupName,
    [Parameter(Position=3, Mandatory=$false)]$Password
  )

  if($ComputerName -eq ".")
  {
    $ComputerName = (get-WmiObject win32_computersystem).Name;
  }    

  $ComputerName = $ComputerName.ToUpper();
  $Domain = @{$true = $env:USERDOMAIN; $false = $env:USERDNSDOMAIN}[$env:USERDNSDOMAIN -eq $null];
  $OnDomain = @{$true = $false; $false = $true}[$env:USERDNSDOMAIN -eq $null];

  if( $UserName.Contains( "\" ) )
  {
    $Domain = Windows-GetDomainFromLogin -LoginName $UserName;
    $UserName = Windows-GetUserNameFromLogin -LoginName $UserName;
    $OnDomain = ($Domain.ToUpper() -ne $ComputerName);
  }

  Write-Host "`$Domain = $Domain";
  Write-Host "`$OnDomain = $OnDomain";
  Write-Host "`$UserName = $UserName";

  if($Domain)
  {
    if( ( Windows-IsUserInLocalGroup -ComputerName $ComputerName -UserName $UserName -LocalGroupName $LocalGroupName ) -eq $false )
    {
      if(-not $OnDomain) {
        # Create the user if it doesn't exist.
        EnsureLocalUser $UserName $Password $ComputerName;
      }
      elseif(-not (DoesUserExistOnDomain $UserName)) {

        # Create the user if it doesn't exist.
        EnsureLocalUser $UserName $Password $ComputerName;
        
        # Add the user to the local group.
        Write-Host "Adding user $($ComputerName)\$($UserName) to $($ComputerName)\$($LocalGroupName).";
        $adsi = [ADSI]"WinNT://$($ComputerName)/$($LocalGroupName),group";
        $adsi.add("WinNT://$($ComputerName)/$($UserName),user");
        
        return;
      }
      
      # Add the user to the local group.
      Write-Host "Adding user $($Domain)\$($UserName) to $($ComputerName)\$($LocalGroupName).";
      $adsi = [ADSI]"WinNT://$($ComputerName)/$($LocalGroupName),group";
      $adsi.add("WinNT://$($Domain)/$($UserName),user");      
    }
    else
    {
      Write-Host "User $($UserName) is already a member of $($ComputerName)\$($LocalGroupName)." -ForegroundColor Green;
    }
  }
  else
  {
    Write-Error "Not connected to a domain.";
  }
}

function EnsureLocalUser
{
  param (
    [Parameter(Position=0, Mandatory=$true)]
    $UserName,
    
    [Parameter(Position=1, Mandatory=$true)]
    $Password,    
    
    [Parameter(Position=2, Mandatory=$true)]
    $ComputerName
  )
  
  $objComputer = [ADSI]("WinNT://$ComputerName, computer");
  $colUsers = ($objComputer.psbase.children |
    Where-Object {$_.psBase.schemaClassName -eq "User"} |
    Select-Object -expand Name)

  $blnFound = $colUsers -contains $UserName

  if ($blnFound) { 
    Write-Host "The user account exists.";
  }
  else {
    Write-Host "The user account does not exist ... creating user $UserName.";
    Windows-CreateLocalUser -UserName $UserName -Password $Password
  }    
}

function DoesUserExistOnDomain
{
  param(
    [Parameter(Position=0, Mandatory=$true)]
    $UserName
  )
  
  $searcher = [adsisearcher]"(samaccountname=$UserName)";
  $rtn = $searcher.findall();

  if($rtn.count -gt 0)
  {
    return $true;
  }
  else
  {
    return $false;
  }

  return $false;
}


function Windows-IsUserInLocalGroup
{
  param (
    [Parameter(Position=0, ValueFromPipeline=$true)]$ComputerName = '.',
    [Parameter(Position=1, Mandatory=$true)]$UserName,
    [Parameter(Position=2, Mandatory=$true)]$LocalGroupName
  )

  if($ComputerName -eq ".")
  {
    $ComputerName = (get-WmiObject win32_computersystem).Name;
  }    

  $ComputerName = $ComputerName.ToUpper();
  $Domain = $env:USERDOMAIN;

  if($Domain)
  {
    $group =[ADSI]"WinNT://$($ComputerName)/$($LocalGroupName)" 
    $members = @($group.psbase.Invoke("Members")) 
    
    if( $members -ne $null) 
    { 
      $member = $members | where { ([ADSI]$_).InvokeGet("Name") -ieq $UserName };
    }
    else
    {
	  $member = $null
    }
    
    return ( $member -ne $null )
  }
  else
  {
    Write-Error "Not connected to a domain.";
  }
}

function Add-NewEventLogSource
{
  param (
    [Parameter(Position=0, Mandatory=$true)]
    $NewSourceName = '.'
  )
  
  if(![System.Diagnostics.EventLog]::SourceExists($NewSourceName)) {
    Write-Host "Creating event log source $($NewSourceName)..." -ForegroundColor Green;
    [System.Diagnostics.EventLog]::CreateEventSource($NewSourceName,'Application')
  }
  else {
    Write-Host "$($NewSourceName) already exists..." -ForegroundColor Yellow;
  }
}
