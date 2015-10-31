#Set-ExecutionPolicy -ExecutionPolicy Unrestricted

param ( 
	[string] $serverName = $(read-host "Enter the SQL Server Name ") ,
	[string] $dbname = $(read-host "Enter the database name to create "),
	[string] $loginAccount = $(read-host "Enter the login account owner (It will create the account if it does not exist) "),
	[string] $pass = $(read-host "Enter a password for the login account owner ")
)

Import-Module “sqlps” -DisableNameChecking

$serverName="(local)"
$dbname="FileStorage"
$loginAccount="FileStorage_User"
$pass="testtest"

# get a connection to the server
$server = new-Object Microsoft.SqlServer.Management.Smo.Server($serverName)

# drop the database if it exists
if ($server.Databases[$dbName] -ne $null)
{  
	$server.KillAllProcesses($dbname)
	$server.Databases[$dbname].drop() 
	Write-Host "Database $dbname was dropped"
} 

# create the database
$db = New-Object Microsoft.SqlServer.Management.Smo.Database($server, $dbname)
$db.Create()
Write-Host "Database $dbname was created on" $db.CreateDate.ToLongDateString()

# create the login account if it doesn't exist
if ($server.Logins[$loginAccount] -eq $null)
{
	$login = new-object Microsoft.SqlServer.Management.Smo.Login($serverName, $loginAccount)
	$login.LoginType = 'SqlLogin'
	$login.PasswordPolicyEnforced = $false
	$login.PasswordExpirationEnabled = $false
	$login.Create($pass)
	Write-Host "Login $loginAccount was created"
}

# set the login account as the owner of the database
$db.SetOwner($loginAccount, $TRUE)
$db.Alter()
Write-Host "Login $loginAccount has been set as the owner of $dbname"