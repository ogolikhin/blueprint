<#
    Build Nova

    Used in Jenkins to build Nova.
    
    Assumptions:
       - None

#>
    


param(
    [Parameter(Mandatory=$false)][string] $workspace = (Get-Item $PSScriptRoot).Parent.Parent.FullName,
    [Parameter(Mandatory=$false)][string] $blueprintVersion = "7.1.0.0",
    [Parameter(Mandatory=$false)][string] $msBuildVerbosity = "m", #q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic].
    [Parameter(Mandatory=$false)][bool] $removeFiles = $true,

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
Build-Nova-Windows-Services @buildParams
Run-Nova-Unit-Tests @buildParams

Build-Nova-Html @buildParams