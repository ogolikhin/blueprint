﻿
-- --------------------------------------------------
-- Migration 8.1.1.0
-- --------------------------------------------------
IF NOT ([FileStore].[IsSchemaVersionLessOrEqual](N'8.1.1') <> 0) 
	set noexec on
Print 'Migrating 8.1.1.0 ...'
-- --------------------------------------------------

-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
IF ([FileStore].[IsSchemaVersionLessOrEqual](N'8.1.1') <> 0)
 	EXEC [FileStore].[SetSchemaVersion] @value = N'8.1.1';
GO
set noexec off
-- --------------------------------------------------