
-- --------------------------------------------------
-- Set the DB
-- --------------------------------------------------
SET QUOTED_IDENTIFIER ON;
GO
USE [ArtifactStorage];
GO
SET NOCOUNT ON;
GO
-- --------------------------------------------------


-- --------------------------------------------------
-- Migration 6.5.1.0
-- --------------------------------------------------
IF NOT ([dbo].[IsSchemaVersionLessOrEqual](N'6.5.1') <> 0) 
	set noexec on
Print 'Migrating 6.5.1.0 ...'
-- --------------------------------------------------

Print 'sample update'

-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
IF ([dbo].[IsSchemaVersionLessOrEqual](N'6.5.1') <> 0)
	EXEC [dbo].[SetSchemaVersion] @value = N'6.5.1';
GO
set noexec off
-- --------------------------------------------------

