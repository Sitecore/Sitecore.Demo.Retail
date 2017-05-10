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

function Clean-Directory
{
  param
    (
        [Parameter(Mandatory=$True)][string]$path

    )
	process
	{
		Write-Verbose "Empty $path"
		Remove-Item $path\* -Recurse
		return 0;
	}
}

function Create-Directory
{
  param
    (
        [Parameter(Mandatory=$True)][string]$path

    )
	process
	{
		Write-Verbose "Creating base directory: $path"
		New-Item -ItemType Directory -Force -Path $path
	}
}
function Copy-SQLDataFiles
{
	param
		(
			[Parameter(Mandatory=$True)][string]$sourcePath,
			[Parameter(Mandatory=$True)][string]$destinationPath,
			[Parameter(Mandatory=$True)][string]$prefix
		)
		begin{}
		process
		{
			gci $sourcePath -filter $prefix* | % { Copy-Item -Path $sourcePath\$_ -Destination $destinationPath -Force }
		}
		end{}
}
Export-ModuleMember Confirm-Resources, Clean-Directory, Create-Directory, Copy-SQLDataFiles