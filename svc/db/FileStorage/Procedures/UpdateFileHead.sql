/******************************************************************************************************************************
Name:			[UpdateFileHead]

Description: 
			
Change History:
Date			Name					Change
2015/11/23		Albert Wong				Initial
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[UpdateFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[UpdateFileHead]
GO

CREATE PROCEDURE [FileStore].[UpdateFileHead]
( 
    @FileId uniqueidentifier,
	@FileSize bigint,
	@ChunkCount int
)
AS
BEGIN

	UPDATE 
		[FileStore].[Files]
    SET
		[FileSize] = @FileSize,
		[ChunkCount] = @ChunkCount 
	WHERE 
		[FileId] = @FileId;
END

GO
