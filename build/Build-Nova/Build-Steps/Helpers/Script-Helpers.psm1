Import-Module "$PSScriptRoot\..\..\Logging-Helpers\Write-JenkinsConsole.psm1"

function Invoke-MsBuild {
    <#
    .SYNOPSIS
        Wrapper for MSBuild

    .NOTES
        Make sure to backtick your semicolons when passing in arguments.
    #>

    param(
        [Parameter(Mandatory=$true)][string]$project,

        [Parameter(Mandatory=$false)][string]$configuration = "ReleaseProtected",
        [Parameter(Mandatory=$false)][string]$teamBuildOutDir = "",
        
        #MSBuild Verbosity: (q[uiet], m[inimal], n[ormal], d[etailed], diag[nostic])
        [Parameter(Mandatory=$false)][string]$verbosity = "m",
        
        [Parameter(Mandatory=$true)][string]$msBuildPath,
        
        [Parameter(Mandatory=$false)][string]$runCodeAnalysis = "false",
        [Parameter(Mandatory=$false)][string]$visualStudioVersion = "12.0",
        [Parameter(Mandatory=$false)][string]$trailingArguments = "",
        [Parameter(Mandatory=$false)][bool]$ignoreErrorCode = $false,
        [Parameter(Mandatory=$false)][bool]$deployOnBuild = $false
    )
    
    $expression = "& '$msBuildPath' /p:RunCodeAnalysis=$runCodeAnalysis /v:$verbosity /p:visualStudioVersion=$visualStudioVersion /p:Configuration=$configuration /p:TeamBuildOutDir=$teamBuildOutDir /p:SkipInvalidConfigurations=true /p:DeployOnBuild=$deployOnBuild $trailingArguments $project"
    
    Write-JenkinsConsole "Invoking msbuild on $project" -ForegroundColor Cyan
    Write-JenkinsConsole "Expression: $expression"
    
    Invoke-Expression $expression

    Write-JenkinsConsole "MSBuild finished with exit code $LastExitCode" -ForegroundColor Magenta

    if($lastexitcode -ne 0 -and -not $ignoreErrorCode){
        throw "MsBuild exited with code $lastexitcode"
    }
}

function Invoke-MyExpression {
    param(
        [Parameter(Mandatory=$true)][string]$expression, 
        [Parameter(Mandatory=$true)]$params, #Not typed so that "" is a valid parameter
        [Parameter(Mandatory=$false)][switch] $ignoreErrorCode,
        [Parameter(Mandatory=$false)][switch] $ignorePositiveErrorCode,
        [Parameter(Mandatory=$false)][Int[]] $successfulExitCodes = (0)
    )

    Write-JenkinsConsole -NoNewline $expression -ForegroundColor Cyan
    Write-JenkinsConsole " $params" -ForegroundColor DarkCyan
    Invoke-Expression "& '$expression' $params"
    
     Write-JenkinsConsole "Exited with code $lastexitcode." -ForegroundColor Cyan
    if($lastexitcode -notin $successfulExitCodes `
        -and -not $ignoreErrorCode `
        -and -not ($ignorePositiveErrorCode -and $lastExitCode -gt 0))
    {
        throw "Invoked expression exited with code $lastexitcode."
    }
}

function Sign-File {
    param(
        [Parameter(Mandatory=$true)][string]$workspace,
        [Parameter(Mandatory=$true)][string]$filename
    )
    
    #We ignore error codes because SigningAutomation.exe returns how many files it signed.
    Invoke-MyExpression "$workspace\build\Tools\SigningAutomation.exe" "file $filename ""C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Bin\signtool.exe"" ""C:\signtool\BP_Code_Signing_Cert_Nov2017.pfx""" -ignorePositiveErrorCode
}

function Sign-FolderContents{
    param(
        [Parameter(Mandatory=$true)][string]$workspace,
        [Parameter(Mandatory=$true)][string]$folder
    )
    
    #We (unfortunately) ignore error codes because SigningAutomation.exe returns how many files it signed
    Invoke-MyExpression "$workspace\build\Tools\SigningAutomation.exe" "folder $folder ""C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Bin\signtool.exe"" ""C:\signtool\BP_Code_Signing_Cert_Nov2017.pfx""" -ignorePositiveErrorCode
}

function Zip-Files {
    param(
        [Parameter(Mandatory=$true)][string]$filename,
        [Parameter(Mandatory=$true)][string]$destination,
        [Parameter(Mandatory=$false)][string]$trailingArgs
    )

    Invoke-MyExpression "C:\Program Files\7-Zip\7z" "-mx5 -tzip -x!*pdb -x!*log  a $destination $filename $trailingArgs"
}

function New-Directory {
    param(
        [Parameter(Mandatory=$true)][string]$directory,
        [Parameter(Mandatory=$false)][bool]$recreate=$false
    )

    if(Test-Path -PathType Container $directory) { 
        if($recreate){
            Write-Info "Recreating $directory"
            Remove-Item -Recurse -Force $directory 
            New-Item -ItemType Directory $directory | Out-Null
        } else {
            Write-Info "Directory $directory already exists."
        }
    } else {
        Write-Info "Creating $directory"
        New-Item -ItemType Directory $directory | Out-Null
    }
}

function Check-TTParity{		
    param(		
        [Parameter(Mandatory=$true)][string]$ttScriptPath,		
        [Parameter(Mandatory=$true)][string]$expectedResultPath,		
        [Parameter(Mandatory=$true)][string]$tempPath		
    )		
 	
        Write-Section "Checking parity between output of $ttPath and contents of $expectedResultPath"		
        		
        #Invoke TextTransform		
        Invoke-MyExpression "C:\Program Files (x86)\Common Files\microsoft shared\TextTemplating\12.0\TextTransform.exe" "$ttScriptPath -out $tempPath"		
 	
        #Compare objects		
        $compareResult = Compare-Object -ReferenceObject (Get-Content $tempPath) -DifferenceObject (Get-Content $expectedResultPath)		
        return $compareResult -eq $null		
 }

  function Modify-SolutionAssemblyVersion {
    param(
        [Parameter(Mandatory=$true)][string]$file,
        [Parameter(Mandatory=$true)][string]$blueprintVersion
    )

    Write-Subsection "Modifying Version in $file"

    $regexAV = "(AssemblyVersion\(\`")(\d+\.\d+\.\d+\.\d+)(\`")";
    $regexFV = "(AssemblyFileVersion\(\`")(\d+\.\d+\.\d+\.\d+)(\`")";
    <# Regex group breakdown:

        (AssemblyVersion\(\")           AssemblyVersion("
        (\d+\.\d+\.\d+\.\d+)            7.0.0.0
        (\")                            "

        We replace group $2 with Blueprint version.
    #>

    $contents = [System.IO.File]::ReadAllText($file)
    $contents = [RegEx]::Replace($contents, $regexAV, "`${1}${blueprintVersion}`$3")
    $contents = [RegEx]::Replace($contents, $regexFV, "`${1}${blueprintVersion}`$3") 
        
    [System.IO.File]::WriteAllText($file, $contents)
}