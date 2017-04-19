﻿
-- -----------------------------------------------------------------------------------------------
-- Migration 8.2.0.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([FileStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	set noexec on
Print 'Migrating 8.2.0.0 ...'
-- -----------------------------------------------------------------------------------------------


-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([FileStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	EXEC [FileStore].[SetSchemaVersion] @value = N'8.2.0';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------
