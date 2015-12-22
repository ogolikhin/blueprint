
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
-- Migration 7.0.0.0
-- --------------------------------------------------
IF NOT ([dbo].[IsSchemaVersionLessOrEqual](N'7.0.0') <> 0) 
	set noexec on
Print 'Migrating 7.0.0.0 ...'
-- --------------------------------------------------



-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
IF ([dbo].[IsSchemaVersionLessOrEqual](N'7.0.0') <> 0)
	EXEC [dbo].[SetSchemaVersion] @value = N'7.0.0';
GO
set noexec off
-- --------------------------------------------------

