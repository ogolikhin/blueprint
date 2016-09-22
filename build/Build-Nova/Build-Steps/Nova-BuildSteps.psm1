#region Imports
Import-Module "$PSScriptRoot\Helpers\Script-Helpers.psm1"
Import-Module "$PSScriptRoot\..\Logging-Helpers\Write-JenkinsConsole.psm1"
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
#endregion

function Setup-Environment {
    param(
        [Parameter(Mandatory=$true)][string]$workspace,
        [Parameter(Mandatory=$true)][bool] $removeFiles, #Recreate files if they already exist

        #Unused, for splatting the same hashtable into multiple methods without error.
        [Parameter(ValueFromRemainingArguments=$true)] $vars
    )

    Write-Section "Setting up environment"

    #Remove and recreate folders
    Write-Subsection "Cleaning up and recreating folders" 
    $folders = @("$workspace\TestResults", "$workspace\Svc\DeployArtifacts", "$workspace\app\NovaWeb\dist")
    $folders | ForEach-Object { 
        New-Directory -directory $_ -recreate $removeFiles
    }

    #Run nuget
    Invoke-MyExpression "c:\nuget.exe" "restore ""$workspace\svc\Services.sln"" -PackagesDirectory ""$workspace/packages"""
}

function Build-Nova-Services{
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

    $sharedTrailingArgs = "/maxcpucount /T:`"Build;WebPublish`" /p:WebPublishMethod=FileSystem /p:DeleteExistingFiles=True /p:AutoParameterizationWebConfigConnectionStrings=False"
    
    Invoke-MsBuild @msBuildArgs -project $workspace\svc\AccessControl\AccessControl.csproj -trailingArguments "/p:publishUrl=`"$workspace\svc\DeployArtifacts\AccessControl`"" + $sharedTrailingArgs
    Invoke-MsBuild @msBuildArgs -project $workspace\svc\ConfigControl\ConfigControl.csproj -trailingArguments "/p:publishUrl=`"$workspace\svc\DeployArtifacts\ConfigControl`"" + $sharedTrailingArgs

    Invoke-MsBuild @msBuildArgs -project $workspace\svc\FileStore\FileStore.csproj -trailingArguments "/p:publishUrl=`"$workspace\svc\DeployArtifacts\FileStore`"" + $sharedTrailingArgs
    Invoke-MsBuild @msBuildArgs -project $workspace\svc\AdminStore\AdminStore.csproj -trailingArguments "/p:publishUrl=`"$workspace\svc\DeployArtifacts\AdminStore`"" + $sharedTrailingArgs
    Invoke-MsBuild @msBuildArgs -project $workspace\svc\ArtifactStore\ArtifactStore.csproj -trailingArguments "/p:publishUrl=`"$workspace\svc\DeployArtifacts\ArtifactStore`"" + $sharedTrailingArgs
    Invoke-MsBuild @msBuildArgs -project $workspace\svc\SearchService\SearchService.csproj -trailingArguments "/p:publishUrl=`"$workspace\svc\DeployArtifacts\SearchService`"" + $sharedTrailingArgs
}

function Build-Nova-Html{
    param(
        [Parameter(Mandatory=$true)][string]$workspace,
        [Parameter(Mandatory=$true)][string]$blueprintVersion,

        [Parameter(Mandatory=$true)][string]$msBuildPath,
        [Parameter(Mandatory=$true)][string]$msBuildVerbosity,
        [Parameter(Mandatory=$true)][string]$visualStudioVersion,
        [Parameter(Mandatory=$false)][bool] $RunTests = $true,

        #Unused, for splatting the same hashtable into multiple methods without error.
        [Parameter(ValueFromRemainingArguments=$true)] $vars
    )

    Write-Section "Building Nova Html"

    try
    {
       pushd "$workspace\app\NovaWeb"
   
       Invoke-MyExpression "npm" "install"
       Invoke-MyExpression "npm" "update"
       Invoke-MyExpression "typings" "i"

       # Increment build version number
       $version = $blueprintVersion.split(".")
       $semver = $version[0] + "." + $version[1] + "." + $version[2] + "-" + $version[3]
       Invoke-MyExpression "npm" "version $semver" -ignoreErrorCode

       # Build Nova Application
       Invoke-MyExpression "npm" "run build"

        if($RunTests)
        {
            # Test Nova Application
            Invoke-MyExpression "npm" "run test"
        }
    }
    finally
    {
        popd
    }
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
    Invoke-MsBuild @msBuildArgs -project $workspace\svc\SearchService.Tests\SearchService.Tests.csproj

    Write-Section "Running tests"
    $vstestArgs =   "`"$workspace\svc\lib\ServiceLibrary.Tests\bin\Release\ServiceLibrary.Tests.dll`" " + 
                    "`"$workspace\svc\AccessControl.Tests\bin\Release\AccessControl.Tests.dll`" " +
                    "`"$workspace\svc\AdminStore.Tests\bin\Release\AdminStore.Tests.dll`" " +
                    "`"$workspace\svc\ArtifactStore.Tests\bin\Release\ArtifactStore.Tests.dll`" " + 
                    "`"$workspace\svc\ConfigControl.Tests\bin\Release\ConfigControl.Tests.dll`" " +
                    "`"$workspace\svc\FileStore.Tests\bin\Release\FileStore.Tests.dll`" " +
                    "`"$workspace\svc\SearchService.Tests\bin\Release\SearchService.Tests.dll`" " +
                    "/Settings:`"$workspace\svc\CodeCoverage.runsettings`" /Enablecodecoverage /UseVsixExtensions:false /Logger:trx"
    Invoke-MyExpression "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" $vstestArgs
}