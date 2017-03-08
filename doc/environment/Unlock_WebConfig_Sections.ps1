Write-Host 'Modify IIS Configuration for Blueprint web site' -foregroundcolor Green

invoke-expression "$Env:WinDir\system32\inetsrv\appcmd.exe unlock config Blueprint -section:system.webServer/serverRuntime -commit:apphost"
invoke-expression "$Env:WinDir\system32\inetsrv\appcmd.exe unlock config Blueprint -section:system.webServer/security/authentication/windowsAuthentication -commit:apphost"
invoke-expression "$Env:WinDir\system32\inetsrv\appcmd.exe unlock config Blueprint -section:system.webServer/security/authentication/basicAuthentication -commit:apphost"
invoke-expression "$Env:WinDir\system32\inetsrv\appcmd.exe unlock config Blueprint -section:system.webServer/security/authentication/digestAuthentication -commit:apphost"
invoke-expression "$Env:WinDir\system32\inetsrv\appcmd.exe unlock config Blueprint -section:system.webServer/security/authentication/anonymousAuthentication -commit:apphost"

Write-Host 'Modify IIS Configuration for Blueprint_Auxiliary web site' -foregroundcolor Green
invoke-expression "$Env:WinDir\system32\inetsrv\appcmd.exe unlock config Blueprint_Auxiliary -section:system.webServer/security/authentication/windowsAuthentication -commit:apphost"
invoke-expression "$Env:WinDir\system32\inetsrv\appcmd.exe unlock config Blueprint_Auxiliary -section:system.webServer/security/authentication/basicAuthentication -commit:apphost"
invoke-expression "$Env:WinDir\system32\inetsrv\appcmd.exe unlock config Blueprint_Auxiliary -section:system.webServer/security/authentication/digestAuthentication -commit:apphost"
invoke-expression "$Env:WinDir\system32\inetsrv\appcmd.exe unlock config Blueprint_Auxiliary -section:system.webServer/security/authentication/anonymousAuthentication -commit:apphost"
