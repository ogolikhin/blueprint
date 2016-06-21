#Code from https://github.com/cpoll/Powershell-ANSI-Color-Script

Import-Module "$PSScriptRoot\Ansi-Colorize.psm1"

function Write-JenkinsConsole {
    <#
        .SYNOPSIS
            Wrapper for Write-Host for use in Jenkins with AnsiColor Plugin.

        .DESCRIPTION
            Adds ANSI SGR codes based on -foregroundColor and -backgroundColor.

            Only does so if it detects a Jenkins environment using Get-InsideJenkinsEnvironment
    #>

    param()

    if(Get-InsideJenkinsEnvironment){
        Write-HostAnsi @Args
    } else {
        Write-Host @Args
    }
}

function Get-InsideJenkinsEnvironment {
    <#
        .SYNOPSIS
            Returns whether the script is being run inside a Jenkins environment.

    #>

    return $Env:JENKINS_URL -ne $null
}

#region Write functions (with ANSI color support)

function Write-Section {
    param($text)
    
    Write-JenkinsConsole (([Environment]::NewLine) + "######## $text ########") -ForegroundColor Magenta    
}

function Write-Info {
    param($text)
    
    Write-JenkinsConsole (([Environment]::NewLine) + "    $text") -ForegroundColor DarkGreen    
}

function Write-Subsection {
    param($text)
    
    Write-JenkinsConsole (([Environment]::NewLine) + "  ----  $text ----") -ForegroundColor DarkMagenta
}
#endregion