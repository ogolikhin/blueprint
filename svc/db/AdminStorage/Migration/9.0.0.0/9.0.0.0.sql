﻿
-- -----------------------------------------------------------------------------------------------
-- Migration 9.0.0.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([AdminStore].[IsSchemaVersionLessOrEqual](N'9.0.0') <> 0) 
	set noexec on
Print 'Migrating 9.0.0.0 ...'
-- -----------------------------------------------------------------------------------------------


-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'9.0.0') <> 0)
	EXEC [AdminStore].[SetSchemaVersion] @value = N'9.0.0';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------
