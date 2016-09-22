#region helpers
function Invoke-MsBuild(){
    param(
       [string]$project = $(throw '-project required'),
       [string]$publishUrl = $(throw '-publishUrl required')
    )

     Invoke-Expression '&"msbuild.exe" $project /maxcpucount /T:Build`;WebPublish /P:Configuration=Release /p:WebPublishMethod=FileSystem /p:DeleteExistingFiles=True /p:publishUrl="$publishUrl"'

}

function Get-ScriptDirectory
{
  $Invocation = (Get-Variable MyInvocation -Scope 1).Value
  Split-Path $Invocation.MyCommand.Path
}
#endregion

$workspace = (get-item $(Get-ScriptDirectory)).Parent.FullName

# Build BP Services
if(Test-Path $WORKSPACE\TestResults){
    Remove-Item -Recurse -Force "$WORKSPACE\TestResults"
}

Invoke-Expression '&"c:\nuget.exe" restore "$WORKSPACE\svc\Services.sln" -PackagesDirectory "$WORKSPACE/svc/packages"'

Invoke-MsBuild -project ${WORKSPACE}\svc\AccessControl\AccessControl.csproj -publishUrl "..\DeployArtifacts\AccessControl"
Invoke-MsBuild -project ${WORKSPACE}\svc\AdminStore\AdminStore.csproj -publishUrl "..\DeployArtifacts\AdminStore"
Invoke-MsBuild -project ${WORKSPACE}\svc\FileStore\FileStore.csproj -publishUrl "..\DeployArtifacts\FileStore"
Invoke-MsBuild -project ${WORKSPACE}\svc\ConfigControl\ConfigControl.csproj -publishUrl "..\DeployArtifacts\ConfigControl"
Invoke-MsBuild -project ${WORKSPACE}\svc\ArtifactStore\ArtifactStore.csproj -publishUrl "..\DeployArtifacts\ArtifactStore"
Invoke-MsBuild -project ${WORKSPACE}\svc\SearchService\SearchService.csproj -publishUrl "..\DeployArtifacts\SearchService"

Read-Host