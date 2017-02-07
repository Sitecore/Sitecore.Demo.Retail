# Add a user or group to an azman role.
function Add-AZAddToRole
{
	param($policyFilePath, $applicationName, $roleName, $domainName, $userGroupName )
	trap 
	{
		Write-Host "An Error has occured.";
		Write-Host "ErrorLevel: $($LastExitCode)";
		Break;
	}
	
	$AZStore = new-object -COMObject AzRoles.AzAuthorizationStore;
	$AZStore.Initialize(0, "msxml://$($policyFilePath)");
	$AZApplication = $AZStore.OpenApplication($applicationName);
	$AZRole = $AZApplication.OpenRole($roleName);
	
	$sid = (Get-WmiObject -Class Win32_UserAccount -ComputerName . -Filter "Domain = '$($domainName)' AND Name = '$($userGroupName)'").SID;
	if( $sid -eq $null )
	{
		$sid = (Get-WmiObject -Class Win32_Group -ComputerName . -Filter "Domain = '$($domainName)' AND Name = '$($userGroupName)'").SID;
	}
	
	if( $sid -eq $null )
	{
		Write-Warning "Unable to add $($domainName)\$($userGroupName) to AzMan role $($roleName) in $($policyFilePath):  Unable to locate user.";
	}
	else
	{
		if( ($AZRole.Members | Where-Object { $_ -eq $sid}) -eq $null )
		{
			$AZRole.AddMember($sid, 0);
			$AZRole.Submit(0, 0);
			Write-Host "User $($domainName)\$($userGroupName) successfully added to $($applicationName) - $($roleName).";
		}
		else
		{
			Write-Host "User $($domainName)\$($userGroupName) already exists in $($applicationName) - $($roleName).";
		}
	}
}

# Add a user or group to an azman role.
function Add-AZAddToRoleInScope
{
	param($policyFilePath, $applicationName, $scopeName, $roleName, $domainName, $userGroupName )
	trap 
	{
		Write-Host "An Error has occured.";
		Write-Host "ErrorLevel: $($LastExitCode)";
		Break;
	}
	
	$AZStore = new-object -COMObject AzRoles.AzAuthorizationStore;
	$AZStore.Initialize(0, "msxml://$($policyFilePath)");
	$AZApplication = $AZStore.OpenApplication($applicationName);
	$AZScope = $AZApplication.OpenScope($scopeName);
	$AZRole = $AZScope.OpenRole($roleName);
	
	$sid = (Get-WmiObject -Class Win32_UserAccount -ComputerName . -Filter "Domain = '$($domainName)' AND Name = '$($userGroupName)'").SID;
	if( $sid -eq $null )
	{
		$sid = (Get-WmiObject -Class Win32_Group -ComputerName . -Filter "Domain = '$($domainName)' AND Name = '$($userGroupName)'").SID;
	}
	
	if( $sid -eq $null )
	{
		Write-Warning "Unable to add $($domainName)\$($userGroupName) to AzMan role $($roleName):$($scopeName) in $($policyFilePath):  Unable to locate user.";
	}
	else
	{
		if( ($AZRole.Members | Where-Object { $_ -eq $sid}) -eq $null )
		{
			$AZRole.AddMember($sid, 0);
			$AZRole.Submit(0, 0);
			Write-Host "User $($domainName)\$($userGroupName) successfully added to $($applicationName) - $($roleName):$($scopeName).";
		}
		else
		{
			Write-Host "User $($domainName)\$($userGroupName) already exists in $($applicationName) - $($roleName):$($scopeName).";
		}
	}
}
function Remove-ReadOnlyOnFiles{
	
	param($WSDirectories )

	Write-Host "Looking for auth files under $($WSDirectories.FullName)...";
	foreach( $dir in $WSDirectories ){

		Write-Host "Looking under $($dir.FullName)...";
		$fullAuthPath = ( Join-Path -Path $dir.FullName -ChildPath "*AuthorizationStore.xml");
		$authFile = Get-Childitem "$fullAuthPath";

		if($authFile -ne $null){
			if($authFile -is [system.array]){
				Write-Host "Found multiple files that match $($fullAuthPath)";

				foreach($auth in $authFile){
					Write-Host "Removing read-only flag on $($auth.FullName)" -ForegroundColor Green;
					$auth.IsReadonly = $false;		
				}
			}
			else{
				Write-Host "Removing read-only flag on $($authFile.FullName)" -ForegroundColor Green;
				$authFile.IsReadonly = $false;
			}
		}
		else{
			Write-Host "Nothing found at $($fullAuthPath)" -ForegroundColor Yellow;
		}
	}
}