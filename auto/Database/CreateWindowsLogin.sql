SET QUOTED_IDENTIFIER OFF;
GO
 
DECLARE @LoginIdentity AS nvarchar(200)
DECLARE @sql AS nvarchar(4000);

-- This name maps to anexisting Windows identity
-- eg: 'IIS APPPOOL\<App Pool Name>', that app pool needs to already exist in IIS.
SET @LoginIdentity = 'IIS APPPOOL\Blueprint' -- REPLACE --
--

SET @SQL = N'USE [master];
IF  EXISTS (SELECT * FROM sys.server_principals WHERE name = N''' + @LoginIdentity + N''')
DROP LOGIN [' + @LoginIdentity + N']'
Exec(@sql);

SET @SQL = N'USE [master];
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE [name] = N''' + @LoginIdentity + N''') 
CREATE LOGIN [' + @LoginIdentity + N'] FROM WINDOWS;'; 
EXEC(@sql);
