
-- -----------------------------------------------------------------------------------------------
-- Migration 7.3.0.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([dbo].[IsSchemaVersionLessOrEqual](N'7.3.0') <> 0) 
	set noexec on
Print 'Migrating 7.3.0.0 ...'
-- -----------------------------------------------------------------------------------------------

-- -----------------------------------------------------------------------------
-- Modify the database filegrowth if it has not changed from the prior defaults
-- -----------------------------------------------------------------------------
DECLARE @db_name AS NVARCHAR(128) = DB_NAME();
DECLARE @sql AS NVARCHAR(max);
DECLARE @file_name AS SYSNAME

SELECT @file_name = d.name FROM sys.database_files d WHERE d.type = 0 AND d.is_percent_growth = 0 AND d.growth = 1280
IF (@file_name IS NOT NULL) 
BEGIN
    SET @sql = N'ALTER DATABASE [' + @db_name + '] MODIFY FILE ( NAME = N''' + @file_name + ''', FILEGROWTH = 10% )'

    EXEC(@sql);
END 

GO

-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([dbo].[IsSchemaVersionLessOrEqual](N'7.3.0') <> 0)
	EXEC [dbo].[SetSchemaVersion] @value = N'7.3.0';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------
