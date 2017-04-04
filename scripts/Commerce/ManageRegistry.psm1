Function GetValueFromRegistryThroughWMI([string]$computername, $regkey, $value, $valueType)    
{    
    #constant for the HLKM    
    $HKLM = "&h80000002"   
   
    #creates an SwbemNamedValueSet object  
    $objNamedValueSet = New-Object -COM "WbemScripting.SWbemNamedValueSet"   
   
    #adds the actual value that will requests the target to provide 64bit-registry info  
    $objNamedValueSet.Add("__ProviderArchitecture", 64) | Out-Null   
   
    #back to all the other usual COM objects for WMI that you have used a zillion times in VBScript  
    $objLocator = New-Object -COM "Wbemscripting.SWbemLocator"   
    $objServices = $objLocator.ConnectServer($computername,"root\default","","","","","",$objNamedValueSet)    
    $objStdRegProv = $objServices.Get("StdRegProv")    
   
    # Obtain an InParameters object specific to the method.    
    $Inparams = ($objStdRegProv.Methods_ | where {$_.name -eq "GetStringValue"}).InParameters.SpawnInstance_()    
    
    # Add the input parameters    
    ($Inparams.Properties_ | where {$_.name -eq "Hdefkey"}).Value = $HKLM   
    ($Inparams.Properties_ | where {$_.name -eq "Ssubkeyname"}).Value = $regkey   
    ($Inparams.Properties_ | where {$_.name -eq "Svaluename"}).Value = $value   
   
    $valueTypeKey = "GetDWORDValue"
    If ($valueType -ieq "String") { $valueTypeKey = "GetStringValue" }

    #Execute the method    
    $Outparams = $objStdRegProv.ExecMethod_($valueTypeKey, $Inparams, "", $objNamedValueSet)
    
    #shows the return value    
    #write-host ($Outparams.Properties_ | where {$_.name -eq "ReturnValue"}).Value    
   
    if (($Outparams.Properties_ | where {$_.name -eq "ReturnValue"}).Value -eq 0)    
    {    
        $valueTypeKey = "uValue"
        If ($valueType -ieq "String") { $valueTypeKey = "sValue" }

        return ($Outparams.Properties_ | where {$_.name -eq $valueTypeKey}).Value
    }
    Else
    {
        throw [System.ArgumentException]("Error, could not find registry key: " + $regkey);
    }
} 

function Get-InternetExplorerEnhancedSecurityEnabled
{
    $computerName = (Get-WmiObject Win32_ComputerSystem).PSComputerName
              
    $adminKey = "SOFTWARE\Microsoft\Active Setup\Installed Components\{A509B1A7-37EF-4b3f-8CFC-4F3A74704073}"
    $userKey = "SOFTWARE\Microsoft\Active Setup\Installed Components\{A509B1A8-37EF-4b3f-8CFC-4F3A74704073}"
 
    Try
    {
        # Need to be able to access 64 bit reg from 32 bit prompt
        $adminIeesValue = GetValueFromRegistryThroughWMI $computerName $adminKey "IsInstalled"
        $userIeesValue = GetValueFromRegistryThroughWMI $computerName $userKey "IsInstalled"

        If(($adminIeesValue -eq 1) -or ($userIeesValue -eq 1))
        {
            return $true
        }
    }
    Catch [System.ArgumentException]
    {
        Write-Verbose "System does not have internet explorer enhanced security. This is usually only valid for servers"
    }

    return $false
}

function Disable-InternetExplorerEnhancedSecurity
{
    # Only works from x64 console.
    $AdminKey = "HKLM:\SOFTWARE\Microsoft\Active Setup\Installed Components\{A509B1A7-37EF-4b3f-8CFC-4F3A74704073}"
    $UserKey = "HKLM:\SOFTWARE\Microsoft\Active Setup\Installed Components\{A509B1A8-37EF-4b3f-8CFC-4F3A74704073}"
    Set-ItemProperty -Path $AdminKey -Name "IsInstalled" -Value 0
    Set-ItemProperty -Path $UserKey -Name "IsInstalled" -Value 0
    Stop-Process -Name Explorer
    Write-Host "IE Enhanced Security Configuration (ESC) has been disabled." -ForegroundColor Green
}

function Disable-LoopbackCheck
{                     
    $key = "REGISTRY::HKLM\SYSTEM\CurrentControlSet\Control\Lsa"
    Set-ItemProperty -Path $key -Name "DisableLoopbackCheck" -Value 1 -Type DWord
}

function Get-WindowsIdentityFoundationEnabled
{
    $computerName = (Get-WmiObject Win32_ComputerSystem).PSComputerName
              
    $key = "SOFTWARE\Microsoft\Windows Identity Foundation\Setup\v3.5"
 
    Try
    {
        $value = GetValueFromRegistryThroughWMI $computerName $key "InstallPath" "String"

        If($value)
        {
            return 0
        }
    }
    Catch [System.ArgumentException]
    {
        Write-Host "System does not have windows identity foundation installed. Please install before trying again" -ForegroundColor red
        Write-Host "Can be done by running 'Install-WindowsFeature windows-identity-foundation' from a 64Bit Powershell session" -ForegroundColor red
    }

    return 1
}

Export-ModuleMember Get-InternetExplorerEnhancedSecurityEnabled, Disable-LoopbackCheck, Get-WindowsIdentityFoundationEnabled