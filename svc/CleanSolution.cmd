@echo off
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S bin') DO DEL /F /Q /S "%%G\*.*"
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"
RMDIR /S /Q ".nuget"
RMDIR /S /Q "packages"
RMDIR /S /Q "TestResults"

echo.
echo Finished...
pause