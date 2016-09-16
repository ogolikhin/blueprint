/******************************************************************************************************************************
Name:			DeleteFileChunk

Description: 
			
Change History:
Date			Name					Change
2015/12/03		Albert					Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteFileChunk]
GO

CREATE PROCEDURE [dbo].[DeleteFileChunk]
(
	@FileId uniqueidentifier,
	@ChunkNumber int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

    DELETE FROM [dbo].[FileChunks] 
    WHERE [FileId] = @FileId AND [ChunkNum] = @ChunkNumber

	SELECT @@ROWCOUNT
END

GO