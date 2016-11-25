function Get-FileAttribute
{
    param($file,$attribute)
    $val = [System.IO.FileAttributes]$attribute;
    if((gci $file -force).Attributes -band $val -eq $val){$true;} else { $false; }
}

function Set-FileAttribute
{
    param($file,$attribute)
    $file =(gci $file -force);
    $file.Attributes = $file.Attributes -band ([System.IO.FileAttributes]$attribute);
}
 
function Clear-FileAttribute
{
    param($file,$attribute)
    $file=(gci $file -force);
    $file.Attributes = $file.Attributes -band (-bnot [System.IO.FileAttributes]$attribute);
}

function Update-XmlAttribute
(
	[Parameter(Mandatory=$true)]
	[string]$FilePath,
	[Parameter(Mandatory=$true)]
	[string]$XPath,
	[Parameter(ParameterSetName="UpdateAttribute", Mandatory=$true)]
	[switch]$UpdateAttribute,
	[Parameter(ParameterSetName="RemoveAttribute", Mandatory=$true)]
	[switch]$RemoveAttribute,
	[Parameter(ParameterSetName="UpdateAttribute", Mandatory=$true)]
	[Parameter(ParameterSetName="RemoveAttribute", Mandatory=$true)]
	$AttributeName,
	[Parameter(ParameterSetName="UpdateAttribute", Mandatory=$true)]
	$AttributeValue,
	[Parameter(ParameterSetName="UpdateAttribute")]
	[switch]$CreateIfNotExists,
	[Parameter(Mandatory=$false)]
	$Namespaces=@{}
)
{
	if( -not ( Test-Path -Path $FilePath ) )
	{
		Write-Error "The file $($FilePath) does not exist.";
	}
	
	$changed = $false;
	
	$content = [xml]( Get-Content -LiteralPath $FilePath );
	
	$foundNode = $null;
	
	if($Namespaces -ne $null){
		$foundNode = $content | Select-Xml -Namespace $Namespaces -XPath $XPath;
	}
	else{
		$foundNode = $content | Select-Xml -XPath $XPath;
	}
		
	$nodeToEdit = $foundNode.Node;
	if( $nodeToEdit )
	{
		Write-Host "Node for XPath $($XPath) has been found.";
		switch( $PsCmdlet.ParameterSetName )
		{
			"UpdateAttribute"
			{
				if( $nodeToEdit.Attributes.GetNamedItem($AttributeName) )
				{
					$attribute = $nodeToEdit.Attributes.GetNamedItem($AttributeName);
					# only change the value if it is currently not the same
					if( $attribute.Value -ne $AttributeValue )
					{
						$attribute.Value = $AttributeValue;
						$changed = $true;
						
						Write-Host "Attribute value $($attributeName) has been updated to $($attributeValue)."
					}
					else
					{
						Write-Host "Attribute value $($attributeName) was already set to $($attributeValue) - No update performed."
					}
				}
				else
				{
					if( $CreateIfNotExists )
					{
						$nodeToEdit.Attributes.Add( $AttributeName, $AttributeValue );
						$changed = $true;
						Write-Host "Attribute value $($attributeName) has been created and set to $($attributeValue)."
					}
					else
					{
						Write-Error "Attribute $($AttributeName) does not exist in the node $($XPath) in file $($FilePath) and, therefore, cannot be updated.";
					}
				}
			}
			"RemoveAttribute"
			{
				$attribute = $nodeToEdit.Attributes.GetNamedItem($AttributeName);
				if( $attribute )
				{
					$nodeToEdit.Attributes.Remove($attribute);
					$changed = $true;
				}
				else
				{
					Write-Error "Attribute $($AttributeName) does not exist - no update performed.";
				}
			}
		}
		
		if( $changed )
		{
			Write-Host "File $($FilePath) has been updated and saved."
			$content.Save($FilePath);
		}
		else
		{
			Write-Host "No updates were performed, so $($FilePath) is untouched."
		}
	}
	else
	{
		Write-Error "Xpath $($XPath) was not found in $($FilePath)";
	}
}