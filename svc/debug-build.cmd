@echo off
if "%VS140COMNTOOLS%" == "" goto VS2013 

:VS2015 
echo VS2015
call "%VS140COMNTOOLS%VsDevCmd.bat"
goto MSBuild

:VS2013
echo VS2013
call "%VS120COMNTOOLS%VsDevCmd.bat"

:MSBuild
set DevDivCodeAnalysisRunType=Disabled
MSBuild Services.sln /t:Build /p:Configuration=Debug /clp:NoItemsAndPropertyList /v:n /m /nologo /p:CodeContractsRunCodeAnalysis=false,RunCodeAnalysis=Never,CodeContractsReferenceAssembly=DoNotBuild %1 %2 %3 %4 %5
pause