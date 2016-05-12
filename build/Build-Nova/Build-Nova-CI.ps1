<#
    Build Nova

    Used in Jenkins to build Nova.
    
    Assumptions:
       - None

#>
    


param(
    [Parameter(Mandatory=$true)][string] $workspace,
    [Parameter(Mandatory=$true)][string] $blueprintVersion,
    [string] $msBuildVerbosity = "m", #q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic].
    [bool] $removeFiles = $true,
    [bool] $buildNovaWeb = $false, #Currently not part of release; remove parameter once it is

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
Run-Nova-Unit-Tests @buildParams

if($buildNovaWeb){
    Build-Nova-Html @buildParams
}