

-- --------------------------------------------------
-- Migration 6.5.1.0
-- --------------------------------------------------
IF NOT ([dbo].[IsSchemaVersionLessOrEqual](N'6.5.1') <> 0) 
	set noexec on
Print 'Migrating 6.5.1.0 ...'
-- --------------------------------------------------

Print 'sample update'

-- --------------------------------------------------
-- Added UpdateFileHead stored procedure
-- --------------------------------------------------
IF  ([dbo].[IsSchemaVersionLessOrEqual](N'6.5.1') <> 0)

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateFileHead]
GO

CREATE PROCEDURE [dbo].[UpdateFileHead]
( 
    @FileId uniqueidentifier,
	@FileSize bigint,
	@ChunkCount int
)
AS
BEGIN

	UPDATE 
		[dbo].[Files]
    SET
		[FileSize] = @FileSize,
		[ChunkCount] = @ChunkCount 
	WHERE 
		[FileId] = @FileId;
END

GO

-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
IF ([dbo].[IsSchemaVersionLessOrEqual](N'6.5.1') <> 0)
	EXEC [dbo].[SetSchemaVersion] @value = N'6.5.1';
GO
set noexec off
-- --------------------------------------------------

