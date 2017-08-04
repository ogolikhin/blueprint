﻿
-- -----------------------------------------------------------------------------------------------
-- Migration 7.4.1.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([AdminStore].[IsSchemaVersionLessOrEqual](N'7.4.1') <> 0) 
	set noexec on
Print 'Migrating 7.4.1.0 ...'
-- -----------------------------------------------------------------------------------------------


-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'7.4.1') <> 0)
	EXEC [AdminStore].[SetSchemaVersion] @value = N'7.4.1';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------
