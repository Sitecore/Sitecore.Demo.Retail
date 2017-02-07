$DEPLOYMENT_DIRECTORY=(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent);

function Expand-String([string] $source)
{
    $source = (($source -replace '&quot;', '`"') -replace '''', '`''');
    if ($PSVersionTable.PSVersion -eq '2.0') {        
        $target = $ExecutionContext.InvokeCommand.ExpandString($source);
    } else {
        
        try {
            # If the source string contains a Powershell expression, we have to use Invoke-Expression because ExpandString is broken in PS v3.0
            if($source.StartsWith('$(') -and $source.EndsWith(')')) {
                Write-Host "Calling Invoke-Expession " $source -ForegroundColor DarkGreen;
                $target = Invoke-Expression $source;
            }
		    else {
                $target =  $ExecutionContext.InvokeCommand.ExpandString($source);
            }
        }
        catch {
            Write-Host 'Assuming $source is a plain old string -> value: ' $source -ForegroundColor DarkGreen;
            $target = $ExecutionContext.InvokeCommand.ExpandString($source);
        }        
    }    
    Write-Host '$target set to ' $target -ForegroundColor Green;
    return $target;
} 
	
function LoadEnvironmentXml
(
	[string]$ConfigurationIdentity="Domain.Dev.Base"
)
{

	$environmentXml = [xml]( Get-Content -Path ( Join-Path -Path $DEPLOYMENT_DIRECTORY -ChildPath "Environment.xml" ) );

	$configuration = $environmentXml.CommerceServer.Configurations.Configuration | where { $_.identity -ieq $ConfigurationIdentity };
	if( $configuration )
	{
		Write-Host "Using configuration $($configuration.identity)";

		Write-Host "Loading variables";
		
		$variables = $configuration.SelectNodes("Variables/Variable");

		foreach( $variable in $variables )
		{
			$variableValue = Expand-String $variable.value

			Write-Host "$($variable.identity) =" $variableValue -ForegroundColor Green;			
			Set-Variable -Name $variable.identity -Value $variableValue -Scope Global;
		}
		foreach( $node in $configuration.SelectNodes( ".//*" ) )
		{
			foreach( $attribute in $node.Attributes )
			{
				# The act of expanding the string here will have side-effects of evaluating $($variable)) and $env:variables.
				$attribute.Value = Expand-String $attribute.Value
			}
		}

		return $configuration;
	}
	else
	{
		Write-Error "No configuration with the identity $($ConfigurationIdentity) was found in the Environment.xml file.";
	}
}
