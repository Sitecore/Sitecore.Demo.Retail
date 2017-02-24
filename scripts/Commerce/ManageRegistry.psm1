Function GetValueFromRegistryThruWMI([string]$computername, $regkey, $value, $valueType)    
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
        Throw "Error, can't find registry value"
    }
} 

function Get-InternetExplorerEnhancedSecurityEnabled
{
    $computerName = (Get-WmiObject Win32_ComputerSystem).PSComputerName
              
    $adminKey = "SOFTWARE\Microsoft\Active Setup\Installed Components\{A509B1A7-37EF-4b3f-8CFC-4F3A74704073}"
    $userKey = "SOFTWARE\Microsoft\Active Setup\Installed Components\{A509B1A8-37EF-4b3f-8CFC-4F3A74704073}"
 
    # Need to be able to access x64 reg from x32 prompt
    $adminIeesValue = GetValueFromRegistryThruWMI $domain $adminKey "IsInstalled"
    $userIeesValue = GetValueFromRegistryThruWMI $domain $userKey "IsInstalled"

    If(($adminIeesValue -eq 1) -or ($userIeesValue -eq 1))
    {
        return $true
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

Export-ModuleMember Get-InternetExplorerEnhancedSecurityEnabled, Disable-LoopbackCheck