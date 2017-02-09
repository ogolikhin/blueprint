﻿
-- -----------------------------------------------------------------------------------------------
-- Migration 8.1.0.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([dbo].[IsSchemaVersionLessOrEqual](N'8.1.0') <> 0) 
	set noexec on
Print 'Migrating 8.1.0.0 ...'
-- -----------------------------------------------------------------------------------------------


-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([dbo].[IsSchemaVersionLessOrEqual](N'8.1.0') <> 0)
	EXEC [dbo].[SetSchemaVersion] @value = N'8.1.0';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------
