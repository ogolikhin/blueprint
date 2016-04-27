#region Imports
Import-Module "$PSScriptRoot\Helpers\Script-Helpers.psm1"
Import-Module "$PSScriptRoot\..\Logging-Helpers\Write-JenkinsConsole.psm1"
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
#endregion

function Setup-Environment {
    param(
        [Parameter(Mandatory=$true)][string]$workspace,
        [bool] $removeFiles, #Recreate files if they already exist

        #Unused, for splatting the same hashtable into multiple methods without error.
        [Parameter(ValueFromRemainingArguments=$true)] $vars
    )

    Write-Section "Setting up environment"

    #Remove and recreate folders
    Write-Subsection "Cleaning up and recreating folders" 
    $folders = @("$workspace\TestResults", "$workspace\svc\DeployArtifacts")
    $folders | ForEach-Object { 
        New-Directory -directory $_ -recreate $removeFiles
    }

    #Run nuget
    Invoke-MyExpression "c:\nuget.exe" "restore ""$workspace\svc\Services.sln"" -PackagesDirectory ""$workspace/packages"""
}

function Build-Nova{
    param(
        [Parameter(Mandatory=$true)][string]$workspace,
        [Parameter(Mandatory=$true)][string]$blueprintVersion,

        [Parameter(Mandatory=$true)][string]$msBuildPath,
        [Parameter(Mandatory=$true)][string]$msBuildVerbosity,
        [Parameter(Mandatory=$true)][string]$visualStudioVersion,

        #Unused, for splatting the same hashtable into multiple methods without error.
        [Parameter(ValueFromRemainingArguments=$true)] $vars
    )

    $msBuildArgs = @{
        verbosity = $msBuildVerbosity
        deployOnBuild = $true 
        configuration = "Release" 
        visualStudioVersion = $visualStudioVersion 
        msbuildPath = $msBuildPath
    }

    Write-Section "Building Nova services"

    Invoke-MsBuild @msBuildArgs -project $workspace\svc\AccessControl\AccessControl.csproj -trailingArguments "/p:publishUrl=`"$workspace\svc\DeployArtifacts\AccessControl`" /maxcpucount /T:`"Build;WebPublish`" /p:WebPublishMethod=FileSystem /p:DeleteExistingFiles=True"
    Invoke-MsBuild @msBuildArgs -project $workspace\svc\ConfigControl\ConfigControl.csproj -trailingArguments "/p:publishUrl=`"$workspace\svc\DeployArtifacts\ConfigControl`" /maxcpucount /T:`"Build;WebPublish`" /p:WebPublishMethod=FileSystem /p:DeleteExistingFiles=True"

    Invoke-MsBuild @msBuildArgs -project $workspace\svc\FileStore\FileStore.csproj -trailingArguments "/p:publishUrl=`"$workspace\svc\DeployArtifacts\FileStore`" /maxcpucount /T:`"Build;WebPublish`" /p:WebPublishMethod=FileSystem /p:DeleteExistingFiles=True"
    Invoke-MsBuild @msBuildArgs -project $workspace\svc\AdminStore\AdminStore.csproj -trailingArguments "/p:publishUrl=`"$workspace\svc\DeployArtifacts\AdminStore`" /maxcpucount /T:`"Build;WebPublish`" /p:WebPublishMethod=FileSystem /p:DeleteExistingFiles=True"
    Invoke-MsBuild @msBuildArgs -project $workspace\svc\ArtifactStore\ArtifactStore.csproj -trailingArguments "/p:publishUrl=`"$workspace\svc\DeployArtifacts\ArtifactStore`" /maxcpucount /T:`"Build;WebPublish`" /p:WebPublishMethod=FileSystem /p:DeleteExistingFiles=True"

}

function Run-Nova-Unit-Tests{
    param(
        [Parameter(Mandatory=$true)][string]$workspace,

        [Parameter(Mandatory=$true)][string]$msBuildPath,
        [Parameter(Mandatory=$true)][string]$msBuildVerbosity,
        [Parameter(Mandatory=$true)][string]$visualStudioVersion,

        #Unused, for splatting the same hashtable into multiple methods without error.
        [Parameter(ValueFromRemainingArguments=$true)] $vars
    )

    Write-Section "Building test files"
    $msBuildArgs = @{
        verbosity = $msBuildVerbosity
        configuration = "Release" 
        visualStudioVersion = $visualStudioVersion 
        msbuildPath = $msBuildPath
        trailingArguments = "/maxcpucount /T:Build"
    }

    Invoke-MsBuild @msBuildArgs -project $workspace\svc\lib\ServiceLibrary.Tests\ServiceLibrary.Tests.csproj

    Invoke-MsBuild @msBuildArgs -project $workspace\svc\AccessControl.Tests\AccessControl.Tests.csproj
    Invoke-MsBuild @msBuildArgs -project $workspace\svc\ConfigControl.Tests\ConfigControl.Tests.csproj
    
    Invoke-MsBuild @msBuildArgs -project $workspace\svc\FileStore.Tests\FileStore.Tests.csproj
    Invoke-MsBuild @msBuildArgs -project $workspace\svc\AdminStore.Tests\AdminStore.Tests.csproj
    Invoke-MsBuild @msBuildArgs -project $workspace\svc\ArtifactStore.Tests\ArtifactStore.Tests.csproj


    Write-Section "Running tests"
    $vstestArgs =   "`"$workspace\lib\ServiceLibrary.Tests\bin\Release\ServiceLibrary.Tests.dll`" " + 
                    "`"$workspace\AccessControl.Tests\bin\Release\AccessControl.Tests.dll`" " +
                    "`"$workspace\AdminStore.Tests\bin\Release\AdminStore.Tests.dll`" " +
                    "`"$workspace\ArtifactStore.Tests\bin\Release\ArtifactStore.Tests.dll`" " + 
                    "`"$workspace\ConfigControl.Tests\bin\Release\ConfigControl.Tests.dll`" " +
                    "`"$workspace\FileStore.Tests\bin\Release\FileStore.Tests.dll`" " +
                    "/Settings:`"$workspace\svc\CodeCoverage.runsettings`" /Enablecodecoverage /UseVsixExtensions:false /Logger:trx"
    Invoke-MyExpression "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" $vstestArgs
}