$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$blueprintDir = (get-item $scriptDir).parent.parent.FullName;
$blueprintCurrentDir = (get-item $scriptDir).parent.parent.Parent.FullName + "\blueprint-current";

Write-Host 'Creating Raptor database'
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $blueprintCurrentDir\Source\BluePrintSys.RC.Data.EntityModel\RecreateRaptor.sql
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $blueprintCurrentDir\Source\BluePrintSys.RC.Data.EntityModel\Instance.sql

Write-Host 'Creating AdminStorage database'
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $blueprintDir\svc\db\AdminStorage\RecreateAdminStorage.sql
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $blueprintDir\svc\db\AdminStorage\AdminStorage_Instance.sql

Write-Host 'Creating ArtifactStorage database'
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $blueprintDir\svc\db\ArtifactStorage\RecreateArtifactStorage.sql
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $blueprintDir\svc\db\ArtifactStorage\ArtifactStorage_Instance.sql

Write-Host 'Creating FileStorage database'
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $blueprintDir\svc\db\FileStorage\RecreateFileStorage.sql
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $blueprintDir\svc\db\FileStorage\FileStorage_Instance.sql

Write-Host 'Creating AppPool logins'
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $scriptDir\CreateBlueprint_AuxiliaryServicesAppPoolLoginForDev.sql
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $scriptDir\CreateBlueprint_PrimaryServicesAppPoolLoginForDev.sql
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $scriptDir\CreateBlueprintAppPoolLoginForDev.sql
