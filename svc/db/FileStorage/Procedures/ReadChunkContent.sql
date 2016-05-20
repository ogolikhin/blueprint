/******************************************************************************************************************************
Name:			[ReadChunkContent]

Description: 
			
Change History:
Date			Name					Change
2015/11/24		CRichards				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReadChunkContent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ReadChunkContent]
GO

CREATE PROCEDURE [dbo].[ReadChunkContent]
( 
    @FileId uniqueidentifier,
    @ChunkNum int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT [ChunkContent]
	FROM [dbo].[FileChunks]
	WHERE [FileId] = @FileId AND [ChunkNum] = @ChunkNum

END

GO