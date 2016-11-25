##################################################################
#
# CreateSolrInstance.ps1
# Creates a new SOLR instance based on the parameters specified
# If index already exists, it will reset the index
#
##################################################################


param(
    [string]$instanceName = $(throw 'The instance name is required, should be the machine name (ws-bld-1203)'),
    [string]$sitecoreVersion = $(throw 'The sitecore version is required (8.0, 8.1, etc'),
    [string]$deploy = $(throw 'The instance deploy function is required')
)

##################################################################
# Global Variables
##################################################################

$TOMCAT_SERVICE_NAME = "TomcatSOLR"
$TOMCAT_INSTALL_DIR = "C:\Tomcat"
$TOMCAT_CONFIG_DIR = "$TOMCAT_INSTALL_DIR\conf\Catalina\localhost"
$TOMCAT_CONFIG_BASEFILE = "$TOMCAT_INSTALL_DIR\conf\Catalina\solr.xml"

$SOLR_INSTALL_DIR = "C:\SOLR\solr-4.10.4"
$SOLR_BUCKET_DIR = "$SOLR_INSTALL_DIR\example"
$SOLR_BUCKET_BASE_DIR = "$SOLR_BUCKET_DIR\solr-base"

$SOLR_BUCKETS_81 = "fxm", "itembuckets_commerce_products_master_index", "itembuckets_commerce_products_web_index", "marketingdefinitions", "sitecore_analytics_index", "sitecore_core_index", "sitecore_fxm_master_index", "sitecore_fxm_web_index", "sitecore_list_index", "sitecore_marketing_asset_index_master", "sitecore_marketing_asset_index_web", "sitecore_master_index", "sitecore_suggested_test_index", "sitecore_testing_index", "sitecore_web_index", "social_messages_master", "social_messages_web"
$SOLR_BUCKETS_82 = "fxm", "itembuckets_commerce_products_master_index", "itembuckets_commerce_products_web_index", "marketingdefinitions", "sitecore_analytics_index", "sitecore_core_index", "sitecore_fxm_master_index", "sitecore_fxm_web_index", "sitecore_list_index", "sitecore_marketing_asset_index_master", "sitecore_marketing_asset_index_web", "sitecore_marketingdefinitions_master", "sitecore_marketingdefinitions_web", "sitecore_master_index", "sitecore_suggested_test_index", "sitecore_testing_index", "sitecore_web_index", "social_messages_master", "social_messages_web"

##################################################################
# Deployment Functions
##################################################################

#
# Deploy-Solr-Instance
#
function Deploy-Solr-Instance
{
    Log -level "h1" -msg "Deploy-Solr-Instace"

    if (Test-Path $solrInstancePath)
    {
        Log -level "err" -msg "Solr Instance $solrInstancePath already exists"
        return
    }

    # Create the new set of buckets using the base buckets
    Log -msg "Creating buckets"
    Copy-Item "$SOLR_BUCKET_BASE_DIR-$sitecoreVersion" $solrInstancePath -Recurse -Force

    # Create XML file
    Log -msg "Creating configuration"
    $solrConfigPath = "$TOMCAT_CONFIG_DIR\solr-$instanceName-$sitecoreVersion.xml"

    Copy-Item $TOMCAT_CONFIG_BASEFILE $solrConfigPath -Force

    # Edit XML File to point to buckets for this instance
    Update-XmlAttribute -FilePath $solrConfigPath -XPath "/Context/Environment" -UpdateAttribute -AttributeName "value" -AttributeValue $solrInstancePath -Namespaces $null

    # Restart Tomcat
    Log -msg "Restarting Tomcat"
    Stop-Service $TOMCAT_SERVICE_NAME
    Start-Service $TOMCAT_SERVICE_NAME
}

#
# Reset-Solr-Instance
#
function Reset-Solr-Instance
{
    Log -level "h1" -msg "Reset-Solr-Instance"

    if (!(Test-Path $solrInstancePath))
    {
        Log -level "err" -msg "Solr Instance does not exist"
        return
    }

    $SOLR_BUCKETS = $SOLR_BUCKETS_81

    if ($sitecoreVersion -eq "8.2")
    {
        $SOLR_BUCKETS = $SOLR_BUCKETS_82
    }

    # Delete each index
    Foreach ($bucket in $SOLR_BUCKETS)
    {
        $bucketIndexPath = "$solrInstancePath\$bucket\data\index"

        try
        {
            Invoke-WebRequest -Uri "http://localhost:8080/solr-$instanceName-$sitecoreVersion/$bucket/update?stream.body=<delete><query>*:*</query></delete>"
            Log -level "success" -msg "Deleting index for $solrServer/$bucket Completed"
        }
        catch
        {
            Log -level "err" -msg "Deleting index for $solrServer/$bucket Failed"
            Log -level "err" -msg $_.Exception.ToString()
        }     
    }

    Log -level "wrn" -msg "Completed reset of each index"
}


##################################################################
# Helper Functions
##################################################################

#
# Log Helper
#
function Log
{
	param(
		[string]$level = "",
		[string]$msg = ""
	)
	
	switch ($level)
	{
		"h1" 
		{ 
			Write-Host ""
			Write-Host "========================================" -ForegroundColor Green
			Write-Host "|| $msg" -ForegroundColor Green
			Write-Host "========================================" -ForegroundColor Green
		}
        "wrn"
        {
            Write-host $msg -ForegroundColor Yellow
        }
		"err" 
		{ 
			Write-Host $msg -ForegroundColor Red
		}
		"success" 
		{ 
			Write-Host $msg -ForegroundColor Green
		}
		default 
		{
			Write-Host $msg -ForegroundColor White
		}
	}
}

#
# Update-Xml Attribute
# Taken from Deployment scripts File.ps1
#
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

##################################################################
# Main
##################################################################
Log -level h1 -msg "Installing CS: Release $releaseNum"


$solrInstancePath = "$SOLR_BUCKET_DIR\solr-$instanceName-$sitecoreVersion".ToLower()

if (Get-Command $deploy -errorAction SilentlyContinue)
{
    . $deploy
}
else
{
    Log -level "err" -msg "Unknown deployment command: $deploy"
}
