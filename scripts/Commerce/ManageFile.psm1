function Confirm-Resources
{
    [CmdletBinding()]
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$resourceList
    )
    begin {}
    process
    {
        foreach ($resource in $resourceList)
        {
            $path = $resource.path;
            if (Test-Path $path) 
            {
                Write-Verbose "Path exists: $path"

                if ((Confirm-File -resource $resource -path $path) -ne 0)
                {
                    return 1;
                }
            }
            else
            {
                Write-Host "Path does not exist: $path" -ForegroundColor red
                return 1;
            }
        }

        return 0;
    }
    end { }
}

function Confirm-File
{
    param 
    (
        [Parameter(Mandatory=$True)][PSCustomObject]$resource,
        [Parameter(Mandatory=$True)][PSCustomObject]$path
    )
    begin {}
    process
    {
        foreach ($file in $resource.files)
        {
            $filename = $file.filename
            if (Test-Path ($path + "\" + $file.filename)) 
            {
            
                Write-Verbose "File exists: $filename"
            }
            else 
            {
                Write-Host "File missing: $filename" -ForegroundColor red
                return 1;
            }
        }

        return 0;
    }
}

Export-ModuleMember Confirm-Resources