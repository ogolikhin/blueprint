﻿
-- --------------------------------------------------
-- Migration 7.3.0.0
-- --------------------------------------------------
IF NOT ([dbo].[IsSchemaVersionLessOrEqual](N'7.3.0') <> 0) 
	set noexec on
Print 'Migrating 7.3.0.0 ...'
-- --------------------------------------------------


-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
IF ([dbo].[IsSchemaVersionLessOrEqual](N'7.3.0') <> 0)
 	EXEC [dbo].[SetSchemaVersion] @value = N'7.3.0';
GO
set noexec off
-- --------------------------------------------------