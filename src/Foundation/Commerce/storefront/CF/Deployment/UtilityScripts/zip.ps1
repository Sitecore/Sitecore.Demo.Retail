# zipping and unzipping
#
# Requires $ZIP_TOOL from environment.xml file

#functions
function UnZipFile
{
	param (
		[String]$sourceFile=$(throw 'Parameter -sourceFile is missing!'), 
        [String]$targetFolder=$(throw 'Parameter -targetFolder is missing!')
	)
    
    if (-not (test-path $($ZIP_TOOL))) {throw "Zip tool not defined, check value of ZIP_TOOL in environment.xml file"} 

	& "$ZIP_TOOL" x $sourceFile "-o$targetFolder"
}

function ZipFolder
{
	param(
		[String]$sourceFolder=$(throw 'Parameter -sourceFolder is missing!'), 
        [String]$targetFile=$(throw 'Parameter -targetFile is missing!')
	)

    if (-not (test-path $($ZIP_TOOL))) {throw "Zip tool not defined, check value of ZIP_TOOL in environment.xml file"} 

	& "$ZIP_TOOL" a -tZip $targetFile ($sourceFolder+"\*")
}
