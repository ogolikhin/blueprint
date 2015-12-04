Import-Module WebAdministration


<#Remaining things to do:

- Add IIS Features
- Configure IIS Security
- Configure Database Security
#>

$blueprintCurrentWorkspacePath = "C:\ws\blueprint-current"
$blueprintNovaWorkspacePath = "C:\ws\blueprint"
$iisRootName = "Blueprint"

$blueprintSiteName = $iisRootName
$blueprintPort = "9801"

$blueprintAppPoolName = $blueprintSiteName

$blueprintAdminSiteName = "${iisRootName}_Admin"
$blueprintAdminPort = "9101"

$servicesAppPoolName = "${iisRootName}_Services"
$iisAppPoolDotNetVersion = "v4.0"

$recreateDatabases = $true #change to $false if you don't want your databases recreated.
$sqlServerInstance = ".\sqlexpress"

#region Create AppPools
#TODO: Loop/function this.

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

#Create Services apppool if it doesn't exist
$appPoolName = $servicesAppPoolName
if (!(Test-Path IIS:\AppPools\$appPoolName -pathType container))
{
    Write-Host "Creating AppPool $appPoolName"
    #create the app pool
    $appPool = New-WebAppPool -name $appPoolName
    $appPool | Set-ItemProperty -Name "managedRuntimeVersion" -Value $iisAppPoolDotNetVersion
} else {
    Write-Host "AppPool $appPoolName already exists"
}


#endregion


#region Create Websites

#Create Blueprint Site
$directoryPath = "$blueprintCurrentWorkspacePath\Source\RequirementsCenter.Web"
if (!(Test-Path IIS:\Sites\$blueprintSiteName -pathType container))
{
    Write-Host "Creating Site $blueprintSiteName"
    $iisApp = New-Website -Name $blueprintSiteName -IPAddress "*" -Port $blueprintPort -physicalPath $directoryPath
    $iisApp | Set-ItemProperty -Name "applicationPool" -Value $blueprintAppPoolName
    #$iisApp | New-WebBinding -IPAddress "*" -Port $blueprintPort #-HostHeader TestSite
} else {
    Write-Host "Site $blueprintSiteName already exists"
}

$directoryPath = "$blueprintNovaWorkspacePath"
if (!(Test-Path IIS:\Sites\$blueprintAdminSiteName -pathType container))
{
    $iisApp = New-Website -Name $blueprintAdminSiteName -IPAddress "*" -Port $blueprintAdminPort -physicalPath $directoryPath
    $iisApp | Set-ItemProperty -Name "applicationPool" -Value $servicesAppPoolName
} else {
    Write-Host "Site $blueprintSiteName already exists"
}

#endregion


#region Create Service Applications under Blueprint

$appPath = "IIS:\Sites\$blueprintSiteName\svc";
$physicalPath = "$blueprintNovaWorkspacePath\svc"

foreach($appName in @("AdminStore","ArtifactStore", "FileStore"))
{

    if((Test-Path "$appPath\$appName") -eq 0 -and (Get-WebApplication -Name $appName) -eq $null)
    {  
        #New-Item -ItemType directory -Path "$physicalPath\$appName"
        New-WebApplication -Name $appName -Site "$blueprintSiteName\svc" -PhysicalPath "$physicalPath\$appName" -ApplicationPool $servicesAppPoolName
    }
    #elseif((Get-WebApplication -Name $appName) -eq $null -and (Test-Path $appPath$appName) -eq $true)
    #{
    #    ConvertTo-WebApplication -ApplicationPool "$appName $appPath$\appName"
    #}
    else
    {
        echo "Application $appName already exists";
    }
}

#endregion 

#region Create Databases

if($recreateDatabases){
    $inputFiles = @(
        # Blueprint Scripts
        #"$blueprintCurrentWorkspacePath\Source\BluePrintSys.RC.Data.EntityModel\RecreateRaptor.sql",
        #"$blueprintCurrentWorkspacePath\Source\BluePrintSys.RC.Data.EntityModel\Instance.sql",
        #"$blueprintCurrentWorkspacePath\Source\BluePrintSys.RC.Data.EntityModel\InitRaptorDBSecurity.sql",

        "$blueprintNovaWorkspacePath\svc\db\AdminStorage\RecreateAdminStorage.sql",
        "$blueprintNovaWorkspacePath\svc\db\AdminStorage\AdminStorage_Instance.sql",

        "$blueprintNovaWorkspacePath\svc\db\ArtifactStorage\RecreateArtifactStorage.sql",
        "$blueprintNovaWorkspacePath\svc\db\ArtifactStorage\ArtifactStorage_Instance.sql",

        "$blueprintNovaWorkspacePath\svc\db\FileStorage\RecreateFileStorage.sql",
        "$blueprintNovaWorkspacePath\svc\db\FileStorage\FileStorage_Instance.sql",

        #Database Security Scripts
        "$blueprintNovaWorkspacePath\doc\environment\CreateBlueprint_ServicesAppPoolLoginForDev.sql",
        "$blueprintNovaWorkspacePath\doc\environment\CreateBlueprintAppPoolLoginForDev.sql"
    )



    foreach($inputFile in $inputFiles){
        Write-Host "Invoking $inputFile on $sqlServerInstance"
        Invoke-Sqlcmd -input $inputFile -serverinstance $sqlServerInstance
    }
}

#endregion