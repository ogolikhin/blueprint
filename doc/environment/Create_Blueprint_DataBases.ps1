$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$blueprintDir = (get-item $scriptDir).parent.parent.FullName;

Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $blueprintDir\svc\db\AdminStorage\RecreateAdminStorage.sql
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $blueprintDir\svc\db\AdminStorage\AdminStorage_Instance.sql
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $blueprintDir\svc\db\ArtifactStorage\RecreateArtifactStorage.sql
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $blueprintDir\svc\db\ArtifactStorage\ArtifactStorage_Instance.sql
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $blueprintDir\svc\db\FileStorage\RecreateFileStorage.sql
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $blueprintDir\svc\db\FileStorage\FileStorage_Instance.sql
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $scriptDir\CreateBlueprint_AuxiliaryServicesAppPoolLoginForDev.sql
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $scriptDir\CreateBlueprint_PrimaryServicesAppPoolLoginForDev.sql
Invoke-Sqlcmd -ServerInstance BlueprintDevDB -InputFile $scriptDir\CreateBlueprintAppPoolLoginForDev.sql
