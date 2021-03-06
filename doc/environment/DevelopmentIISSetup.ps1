Import-Module WebAdministration

#prompt for input
$inputIisRootName = $(read-host "`r`nEnter the root name for the Blueprint web sites `r`n(Default: Blueprint ) ") 
$inputBlueprintPort = $(read-host "`r`nEnter the port of the Blueprint web site `r`n(Default: 9801 ) ") 
$inputBlueprintAuxiliaryPort = $(read-host "`r`nEnter the port of the Blueprint Auxiliary web site `r`n(Default: 9101 ) ") 
$inputDeleteWebsites = $(Read-Host "`r`nDelete websites if they already exist `r`n(True/False Default: True ) ")
$blueprintNovaWorkspacePath = $(read-host "`r`nEnter the location of your Blueprint repo") 
$blueprintCurrentWorkspacePath = $(read-host "`r`nEnter the location of your Blueprint-Current repo") 

# Declare variables with default values
$iisRootName 								= "Blueprint"
$blueprintPort 								= "9801"
$blueprintAuxiliaryPort 					= "9101"
$deleteWebsites								= $true

# Modify variables from user input
if ($inputIisRootName) {$iisRootName = [string]$inputIisRootName}
if ($inputBlueprintPort) {$blueprintPort = [string]$inputBlueprintPort}
if ($inputBlueprintAuxiliaryPort) {$blueprintAuxiliaryPort = [string]$inputBlueprintAuxiliaryPort}
if ($inputDeleteWebsites -eq "false") { $deleteWebsites = $false }

$blueprintSiteName = $iisRootName
$blueprintAppPoolName = $blueprintSiteName
$blueprintAuxiliarySiteName = "${iisRootName}_Auxiliary"
$primaryServicesAppPoolName = "${iisRootName}_PrimaryServices"
$auxiliaryServicesAppPoolName = "${iisRootName}_AuxiliaryServices"
$iisAppPoolDotNetVersion = "v4.0"

#$blueprintCurrentWorkspacePath = "C:\Users\cdufour\Repos\blueprint-current"
#$blueprintNovaWorkspacePath = "C:\Users\cdufour\Repos\blueprint"

Write-Host ""

#region Delete websites 

if ($deleteWebsites)
{
	if (Test-Path IIS:\Sites\$blueprintSiteName -pathType container)
	{
		Remove-Website -Name $blueprintSiteName
		Write-Host "Deleted Site $blueprintSiteName"
	}
	if (Test-Path IIS:\Sites\$blueprintAuxiliarySiteName -pathType container)
	{
		Remove-Website -Name $blueprintAuxiliarySiteName
		Write-Host "Deleted Site $blueprintAuxiliarySiteName"
	}
	if (Test-Path IIS:\AppPools\$blueprintAppPoolName -pathType container)
	{
		Remove-WebAppPool -Name $blueprintAppPoolName
		Write-Host "Deleted AppPool $blueprintAppPoolName"
	}
	if (Test-Path IIS:\AppPools\$primaryServicesAppPoolName -pathType container)
	{
		Remove-WebAppPool -Name $primaryServicesAppPoolName
		Write-Host "Deleted AppPool $primaryServicesAppPoolName"
	}
	if (Test-Path IIS:\AppPools\$auxiliaryServicesAppPoolName -pathType container)
	{
		Remove-WebAppPool -Name $auxiliaryServicesAppPoolName
		Write-Host "Deleted AppPool $auxiliaryServicesAppPoolName"
	}
	Write-Host ""
}

#endregion

#region Create AppPools

#Create Blueprint apppool if it doesn't exist
$appPoolName = $blueprintAppPoolName
if (!(Test-Path IIS:\AppPools\$appPoolName -pathType container))
{
    Write-Host "Creating AppPool $appPoolName"
    #create the app pool
    $appPool = New-WebAppPool -name $appPoolName
    $appPool | Set-ItemProperty -Name "managedRuntimeVersion" -Value $iisAppPoolDotNetVersion
    $appPool | Set-ItemProperty -Name "enable32BitAppOnWin64" -Value "true"
} else {
    Write-Host "AppPool $appPoolName already exists"
}

#Create Primary Services apppool if it doesn't exist
$appPoolName = $primaryServicesAppPoolName
if (!(Test-Path IIS:\AppPools\$appPoolName -pathType container))
{
    Write-Host "Creating AppPool $appPoolName"
    #create the app pool
    $appPool = New-WebAppPool -name $appPoolName
    $appPool | Set-ItemProperty -Name "managedRuntimeVersion" -Value $iisAppPoolDotNetVersion
} else {
    Write-Host "AppPool $appPoolName already exists"
}

#Create Auxiliary Services apppool if it doesn't exist
$appPoolName = $auxiliaryServicesAppPoolName
if (!(Test-Path IIS:\AppPools\$appPoolName -pathType container))
{
    Write-Host "Creating AppPool $appPoolName"
    #create the app pool
    $appPool = New-WebAppPool -name $appPoolName
    $appPool | Set-ItemProperty -Name "managedRuntimeVersion" -Value $iisAppPoolDotNetVersion
} else {
    Write-Host "AppPool $appPoolName already exists"
}

Write-Host ""

#endregion

#region Create Blueprint Website

#Create Blueprint Site
$directoryPath = "$blueprintCurrentWorkspacePath\Source\RequirementsCenter.Web"
if (!(Test-Path IIS:\Sites\$blueprintSiteName -pathType container))
{
    Write-Host "Creating Site $blueprintSiteName"
    $iisApp = New-Website -Name $blueprintSiteName -IPAddress "*" -Port $blueprintPort -physicalPath $directoryPath
    $iisApp | Set-ItemProperty -Name "applicationPool" -Value $blueprintAppPoolName
    #$iisApp | New-WebBinding -IPAddress "*" -Port $blueprintPort #-HostHeader TestSite
#region Modify IIS Configuration

	Write-Host "Modifying IIS Configuration"

	$assembly = [System.Reflection.Assembly]::LoadFrom("$env:systemroot\system32\inetsrv\Microsoft.Web.Administration.dll")
	$oIISMgr = new-object Microsoft.Web.Administration.ServerManager
	$oGlobalConfig = $oIISMgr.GetApplicationHostConfiguration()

	$oConfig = $oGlobalConfig.GetSection("system.webServer/serverRuntime", $blueprintSiteName)
	$oConfig.OverrideMode="Allow"

	$oConfig = $oGlobalConfig.GetSection("system.webServer/security/authentication/anonymousAuthentication", $blueprintSiteName)
	$oConfig.OverrideMode="Allow"

	$oConfig = $oGlobalConfig.GetSection("system.webServer/security/authentication/basicAuthentication", $blueprintSiteName)
	$oConfig.OverrideMode="Allow"

	$oConfig = $oGlobalConfig.GetSection("system.webServer/security/authentication/digestAuthentication", $blueprintSiteName)
	$oConfig.OverrideMode="Allow"

	$oConfig = $oGlobalConfig.GetSection("system.webServer/security/authentication/windowsAuthentication", $blueprintSiteName)
	$oConfig.OverrideMode="Allow"

	$oIISMgr.CommitChanges()

#endregion
	
} else {
    Write-Host "Site $blueprintSiteName already exists"
}

#region Create Service Applications under Blueprint

$appPath = "IIS:\Sites\$blueprintSiteName\svc";
$physicalPath = "$blueprintNovaWorkspacePath\svc"

Write-Host "Creating Service applications for $blueprintSiteName"
foreach($appName in @("AdminStore","ArtifactStore", "FileStore", "SearchService"))
{

    if((Get-WebApplication -Name "svc/$appName" -Site $blueprintSiteName) -eq $null)
    {  
        New-WebApplication -Name $appName -Site "$blueprintSiteName\svc" -PhysicalPath "$physicalPath\$appName" -ApplicationPool $primaryServicesAppPoolName
    }
    else
    {
        echo "Application $appName already exists";
    }
}

#endregion 

Write-Host ""

#endregion

#region Create Blueprint Auxiliary Website

$directoryPath = "$blueprintNovaWorkspacePath"
if (!(Test-Path IIS:\Sites\$blueprintAuxiliarySiteName -pathType container))
{
    Write-Host "Creating Site $blueprintAuxiliarySiteName"
    $iisApp = New-Website -Name $blueprintAuxiliarySiteName -IPAddress "*" -Port $blueprintAuxiliaryPort -physicalPath $directoryPath
    $iisApp | Set-ItemProperty -Name "applicationPool" -Value $auxiliaryServicesAppPoolName
#region Modify IIS Configuration

	Write-Host "Modifying IIS Configuration"

	$assembly = [System.Reflection.Assembly]::LoadFrom("$env:systemroot\system32\inetsrv\Microsoft.Web.Administration.dll")
	$oIISMgr = new-object Microsoft.Web.Administration.ServerManager
	$oGlobalConfig = $oIISMgr.GetApplicationHostConfiguration()

	$oConfig = $oGlobalConfig.GetSection("system.webServer/serverRuntime", $blueprintAuxiliarySiteName)
	$oConfig.OverrideMode="Allow"

	$oConfig = $oGlobalConfig.GetSection("system.webServer/security/authentication/anonymousAuthentication", $blueprintAuxiliarySiteName)
	$oConfig.OverrideMode="Allow"

	$oConfig = $oGlobalConfig.GetSection("system.webServer/security/authentication/basicAuthentication", $blueprintAuxiliarySiteName)
	$oConfig.OverrideMode="Allow"

	$oConfig = $oGlobalConfig.GetSection("system.webServer/security/authentication/digestAuthentication", $blueprintAuxiliarySiteName)
	$oConfig.OverrideMode="Allow"

	$oConfig = $oGlobalConfig.GetSection("system.webServer/security/authentication/windowsAuthentication", $blueprintAuxiliarySiteName)
	$oConfig.OverrideMode="Allow"

	$oIISMgr.CommitChanges()

#endregion

} else {
    Write-Host "Site $blueprintSiteName already exists"
}

#region Create Service Applications under Blueprint Auxiliary

$appPath = "IIS:\Sites\$blueprintAuxiliarySiteName\svc";
$physicalPath = "$blueprintNovaWorkspacePath\svc"

Write-Host "Creating Service applications for $blueprintAuxiliarySiteName"
foreach($appName in @("AccessControl", "ConfigControl"))
{

    if((Get-WebApplication -Name "svc/$appName" -Site $blueprintAuxiliarySiteName) -eq $null)
    {  
        New-WebApplication -Name $appName -Site "$blueprintAuxiliarySiteName\svc" -PhysicalPath "$physicalPath\$appName" -ApplicationPool $auxiliaryServicesAppPoolName
    }
    else
    {
        echo "Application $appName already exists";
    }
}

#endregion 

Write-Host ""

#endregion
