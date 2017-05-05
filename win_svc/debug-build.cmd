@echo off
if "%VS140COMNTOOLS%" == "" goto VS2013 

:VS2015 
echo VS2015
call "%VS140COMNTOOLS%VsDevCmd.bat"
goto setupnuget

:VS2013
echo VS2013
call "%VS120COMNTOOLS%VsDevCmd.bat"

:setupnuget
SETLOCAL
SET NUGET_VERSION=latest
SET CACHED_NUGET=%LocalAppData%\NuGet\nuget.%NUGET_VERSION%.exe
SET MSBUILDDISABLENODEREUSE=1

IF EXIST %CACHED_NUGET% goto copynuget
echo Downloading latest version of NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://dist.nuget.org/win-x86-commandline/%NUGET_VERSION%/nuget.exe' -OutFile '%CACHED_NUGET%'"

:copynuget
IF EXIST .nuget\nuget.exe goto MSBuild
md .nuget
copy %CACHED_NUGET% .nuget\nuget.exe > nul

:MSBuild
.nuget\nuget.exe restore

set DevDivCodeAnalysisRunType=Disabled
MSBuild WindowsServices.sln /t:Build /p:Configuration=Debug /p:Platform=x64 /clp:NoItemsAndPropertyList /v:n /m /nologo /p:CodeContractsRunCodeAnalysis=false,RunCodeAnalysis=Never,CodeContractsReferenceAssembly=DoNotBuild %1 %2 %3 %4 %5
pause