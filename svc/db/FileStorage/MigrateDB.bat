"..\..\..\svc\packages\FluentMigrator.Tools.1.6.0\tools\x86\40\Migrate.exe" -tag=DBSetup --version=0 -db SqlServer2012 -conn "Data Source=(local);Initial Catalog=FileStorage;Persist Security Info=True;integrated security=True;" -a bin\debug\FileStorage.dll --output --outputFilename migrated.sql
Pause