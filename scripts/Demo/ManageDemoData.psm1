Import-Module WebAdministration

function Export-MongoData
{
    param 
    (
		[Parameter(Mandatory=$True)][string]$mongoBinDirectory,
        [Parameter(Mandatory=$True)][string]$outputDirectory
    )

    begin 
    {
        Write-Verbose "Exporting Mongo Data to: $outputDirectory"
    }
    process
    {
		$mongodump = $mongoBinDirectory + "\mongodump.exe"
		& $mongodump --db "habitat821_analytics" -o $outputDirectory --forceTableScan
		& $mongodump --db "habitat821_tracking_contact" -o $outputDirectory --forceTableScan
		& $mongodump --db "habitat821_tracking_history" -o $outputDirectory --forceTableScan
		& $mongodump --db "habitat821_tracking_live" -o $outputDirectory --forceTableScan
		
		Get-ChildItem -Path $outputDirectory   | Rename-Item -NewName  { $_.name -replace 'habitat821_','' }
	

    }
    end
    {
    }
}
function Import-MongoData
{
    param 
    (
		[Parameter(Mandatory=$True)][string]$mongoBinDirectory,
        [Parameter(Mandatory=$True)][string]$inputDirectory
    )

    begin 
    {
        Write-Verbose "Exporting Mongo Data to: $inputDirectory"
    }
    process
    {
		$mongorestore = $mongoBinDirectory + "\mongorestore.exe"
		& $mongorestore --db "habitat821_analytics"  "$inputDirectory\analytics"  --drop
		& $mongorestore --db "habitat821_tracking_contact"  "$inputDirectory\tracking_contact" --drop
		& $mongorestore --db "habitat821_tracking_history"  "$inputDirectory\tracking_history" --drop
		& $mongorestore --db "habitat821_tracking_live"  "$inputDirectory\tracking_live" --drop
    }
    end
    {
    }
}

Export-ModuleMember  Export-MongoData, Import-MongoData