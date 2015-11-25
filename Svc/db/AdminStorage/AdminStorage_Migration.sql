
-- --------------------------------------------------
-- Set the DB
-- --------------------------------------------------
SET QUOTED_IDENTIFIER ON;
GO
USE [Blueprint_AdminStorage]; -- REPLACE --
GO
SET NOCOUNT ON;
GO
-- --------------------------------------------------


-- --------------------------------------------------
-- Migration 7.0.1.0
-- --------------------------------------------------
IF NOT ([dbo].[IsSchemaVersionLessOrEqual](N'7.0.1') <> 0) 
	set noexec on
Print 'Migrating 7.0.1.0 ...'
-- --------------------------------------------------

Print 'sample update'

-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
IF ([dbo].[IsSchemaVersionLessOrEqual](N'7.0.1') <> 0)
	EXEC [dbo].[SetSchemaVersion] @value = N'7.0.1';
GO
set noexec off
-- --------------------------------------------------

