
-- --------------------------------------------------
-- Migration 7.1.0.0
-- --------------------------------------------------
SET QUOTED_IDENTIFIER ON;
GO
USE [Blueprint_FileStorage]; -- REPLACE --
GO
SET NOCOUNT ON;
GO
IF NOT ([dbo].[IsSchemaVersionLessOrEqual](N'7.1.0') <> 0) 
	set noexec on
Print 'Migrating 7.1.0.0 ...'
-- --------------------------------------------------


-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
IF ([dbo].[IsSchemaVersionLessOrEqual](N'7.1.0') <> 0)
 	EXEC [dbo].[SetSchemaVersion] @value = N'7.1.0';
GO
set noexec off
-- --------------------------------------------------