<#
    Build Nova

    Used in Jenkins to build Nova.
    
    Assumptions:
       - None

#>
    


param(
    [Parameter(Mandatory=$true)][string] $workspace,
    [Parameter(Mandatory=$true)][string] $blueprintVersion,
    [Parameter(Mandatory=$false)][string] $msBuildVerbosity = "m", #q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic].
    [Parameter(Mandatory=$false)][bool] $removeFiles = $true,
    [Parameter(Mandatory=$false)][bool] $buildNovaWeb = $false, #Currently not part of release; remove parameter once it is
    [Parameter(Mandatory=$false)][bool] $RunTests = $true,

    #Unused, for splatting the same hashtable into multiple methods without error.
    [Parameter(ValueFromRemainingArguments=$true)] $vars
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

#region Imports
Import-Module "$PSScriptRoot\Logging-Helpers\Write-JenkinsConsole.psm1"
Import-Module "$PSScriptRoot\Build-Steps\Nova-BuildSteps.psm1"
#endregion

$buildParams = @{
    workspace = $workspace
    blueprintVersion = $blueprintVersion
    msBuildVerbosity = $msBuildVerbosity
    msBuildPath = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
    visualStudioVersion = "14.0"
}

Setup-Environment @buildParams -removeFiles $removeFiles
Build-Nova-Services @buildParams

if($RunTests)
{
    Run-Nova-Unit-Tests @buildParams
}

if($buildNovaWeb){
    Build-Nova-Html @buildParams -RunTests $RunTests
}