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

    #Run nuget for services
    Invoke-MyExpression "c:\nuget.exe" "restore ""$workspace\win_svc\WindowsServices.sln"" -PackagesDirectory ""$workspace/packages"""
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

    Modify-SolutionAssemblyVersion -file $workspace\svc\SolutionAssemblyVersion.cs -blueprintVersion $BlueprintVersion

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
        [Parameter(Mandatory=$false)][bool] $BuildDebug = $false,

        #Unused, for splatting the same hashtable into multiple methods without error.
        [Parameter(ValueFromRemainingArguments=$true)] $vars
    )

    Write-Section "Building Nova Html"

    try
    {
        pushd "$workspace\app\NovaWeb"
   
        Invoke-MyExpression "yarn" "install"
        # Invoke-MyExpression "yarn" "upgrade"

        # Increment build version number
        $version = $blueprintVersion.split(".")
        $semver = $version[0] + "." + $version[1] + "." + $version[2] + "-" + $version[3]
        Invoke-MyExpression "yarn" "version --new-version $semver" -ignoreErrorCode

        # Build Nova Application
        if($BuildDebug) {
            Invoke-MyExpression "yarn" "run build -- --debug"
        } else {
            Invoke-MyExpression "yarn" "run build"
        }

        if($RunTests)
        {
            # Test Nova Application
            Invoke-MyExpression "yarn" "run test"
        }
    }
    finally
    {
        popd
    }
}

<#
    Builds new windows services
    
    ImageRenderService is dependant on files in the NovaWeb dist folder, so should be run after building that. 
#>
function Build-Nova-Windows-Services{
    param(
        [Parameter(Mandatory=$true)][string]$workspace,
        [Parameter(Mandatory=$true)][string]$blueprintVersion,

        [Parameter(Mandatory=$true)][string]$msBuildPath,
        [Parameter(Mandatory=$true)][string]$msBuildVerbosity,
        [Parameter(Mandatory=$true)][string]$visualStudioVersion,

        #Unused, for splatting the same hashtable into multiple methods without error.
        [Parameter(ValueFromRemainingArguments=$true)] $vars
    )

    $buildParams = @{
        workspace = $workspace
        msBuildVerbosity = $msBuildVerbosity
        configuration = "Release" 
        visualStudioVersion = $visualStudioVersion 
        msbuildPath = $msBuildPath
    }

    Write-Section "Building Nova Windows services"
    
    Build-ImageService @buildParams
    
    Build-BlueprintServices @buildParams 
}

function Build-ImageService{
    param(
        [Parameter(Mandatory=$true)][string]$workspace,

        [Parameter(Mandatory=$true)][string]$msBuildPath,
        [Parameter(Mandatory=$true)][string]$msBuildVerbosity,
        [Parameter(Mandatory=$true)][string]$visualStudioVersion,
        [Parameter(Mandatory=$true)][string]$configuration
    )
    
    $msBuildArgs = @{
        verbosity = $msBuildVerbosity
        configuration = "Release" 
        visualStudioVersion = $visualStudioVersion 
        msbuildPath = $msBuildPath
    }

    Write-Section "Building Image Service"

    Invoke-MsBuild @msBuildArgs -project $workspace\win_svc\BlueprintSys.RC.ImageService\BlueprintSys.RC.ImageService.csproj -trailingArguments "/p:Platform='x64' /p:OutDir=`"$workspace\win_svc\DeployArtifacts\BlueprintSys.RC.ImageService`""

    $processHtmlFolder = "$workspace\win_svc\DeployArtifacts\BlueprintSys.RC.ImageService\ProcessHtml"
    $novaImageGenFolder = "$workspace\app\NovaWeb\dist\imagegen"

    if(Test-Path -PathType Container $processHtmlFolder) {
        Write-Section "Removing hard-coded folder at path '$processHtmlFolder'"
        Remove-Item -Recurse -Force $processHtmlFolder 
    }

    Write-Section "Copying NovaWeb imagegen files ($novaImageGenFolder) to $processHtmlFolder"
    New-Item -ItemType Directory $processHtmlFolder | Out-Null
    Copy-Item -Destination $processHtmlFolder -Path "$novaImageGenFolder\*" -Recurse -WarningAction Continue -ErrorAction Continue

    Write-Section "Removing $novaImageGenFolder"
    if(Test-Path -PathType Container $novaImageGenFolder) {
        Remove-Item -Recurse -Force $novaImageGenFolder 
    }
}

function Build-BlueprintServices{
    param(
        [Parameter(Mandatory=$true)][string]$workspace,

        [Parameter(Mandatory=$true)][string]$msBuildPath,
        [Parameter(Mandatory=$true)][string]$msBuildVerbosity,
        [Parameter(Mandatory=$true)][string]$visualStudioVersion,
        [Parameter(Mandatory=$true)][string]$configuration
    )
    
    $msBuildArgs = @{
        verbosity = $msBuildVerbosity
        configuration = "Release" 
        visualStudioVersion = $visualStudioVersion 
        msbuildPath = $msBuildPath
    }

    Write-Section "Building Blueprint Services"

    Invoke-MsBuild @msBuildArgs -project $workspace\win_svc\BlueprintSys.RC.Services\BlueprintSys.RC.Services.csproj -trailingArguments "/p:Platform='x64' /p:OutDir=`"$workspace\win_svc\DeployArtifacts\BlueprintSys.RC.Services`""
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
