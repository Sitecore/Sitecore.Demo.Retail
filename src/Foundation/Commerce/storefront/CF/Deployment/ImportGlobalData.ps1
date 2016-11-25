# Import the Commerce Entities in the Global database
$headers = "Id","EnvironmentId","Version","Entity";
$data = Import-Csv -Delimiter ("|") -Encoding UTF8 -Path C:\SRC\Commerce.Storefront\CF\Deployment\GlobalCommerceEntities.csv -Header $headers

foreach ($Line in $data)

{
    $Id=”`'”+$Line.Id+”`'”
    $EnvironmentId=”`'”+$Line.EnvironmentId+”`'”
    $Version=”`'”+$Line.Version+”`'”
    $Entity=”`'”+$Line.Entity+”`'”

    $SQLHEADER=”INSERT INTO SitecoreCommerce_Global_Copy.dbo.CommerceEntities ([Id],[EnvironmentId],[Version],[Entity])“

    $SQLVALUES=”VALUES ($Id,$EnvironmentId,$Version,$Entity)”

    $SQLQUERY=$SQLHEADER+$SQLVALUES
    #Write-Host $SQLQUERY

    Invoke-Sqlcmd –Query $SQLQuery -ServerInstance localhost

}

# Import the Commerce Lists into the Global Database
$headers = "ListName","EnvironmentId","CommerceEntityId";
$data = Import-Csv -Delimiter ("|") -Encoding UTF8 -Path C:\SRC\Commerce.Storefront\CF\Deployment\GlobalCommerceLists.csv -Header $headers

foreach ($Line in $data)

{
    $ListName=”`'”+$Line.ListName+”`'”
    $EnvironmentId=”`'”+$Line.EnvironmentId+”`'”
    $CommerceEntityId=”`'”+$Line.CommerceEntityId+”`'”

    $SQLHEADER=”INSERT INTO SitecoreCommerce_Global_Copy.dbo.CommerceLists ([ListName],[EnvironmentId],[CommerceEntityId])“

    $SQLVALUES=”VALUES ($ListName,$EnvironmentId,$CommerceEntityId)”

    $SQLQUERY=$SQLHEADER+$SQLVALUES
    #Write-Host $SQLQUERY

    Invoke-Sqlcmd –Query $SQLQuery -ServerInstance localhost

}