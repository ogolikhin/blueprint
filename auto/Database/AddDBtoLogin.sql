SET QUOTED_IDENTIFIER OFF;
GO
 
DECLARE @DBName AS nvarchar(200);
DECLARE @LoginIdentity AS nvarchar(200)
DECLARE @sql AS nvarchar(4000);

-- This name maps to an existing Windows identity
-- eg: 'IIS APPPOOL\<App Pool Name>', that app pool needs to already exist in IIS.
SET @LoginIdentity = 'IIS APPPOOL\Blueprint' -- REPLACE --
-- This is the name of the database
SET @DBName = N'Blueprint'; -- REPLACE --
--

SET @SQL = N'USE [' + @DBName + ']; 
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N''' + @LoginIdentity + N''')
DROP USER [' + @LoginIdentity + ']'
EXEC(@sql);

SET @SQL = N'USE [' + @DBName + ']; 
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE [name] = ''' + @LoginIdentity + N''') 
CREATE USER [' + @LoginIdentity + '] FOR LOGIN [' + @LoginIdentity + '];';
EXEC(@sql);

SET @SQL = N'USE [' + @DBName + ']; 
ALTER ROLE [db_blueprint_reader] ADD MEMBER ['+ @LoginIdentity + N'];
ALTER ROLE [db_blueprint_writer] ADD MEMBER ['+ @LoginIdentity + N'];
ALTER ROLE [db_blueprint_executor] ADD MEMBER ['+ @LoginIdentity + N'];'
EXEC(@SQL)

