:: This needs to run in a command prompt with elevated privileges
:: You will need to set the Powershell execution policy
::     Set-ExecutionPolicy -ExecutionPolicy Unrestricted

Powershell ".\..\StorageUtils\CreateDB.ps1" "." "FileStorage" "FileStorage_User" "testtest"
