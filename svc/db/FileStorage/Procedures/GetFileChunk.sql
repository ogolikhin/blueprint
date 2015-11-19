﻿/******************************************************************************************************************************
Name:			[GetFileChunk]

Description: 
			
Change History:
Date			Name					Change
2015/11/19		Albert Wong				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetFileChunk]
GO

CREATE PROCEDURE [dbo].[GetFileChunk]
( 
    @FileId uniqueidentifier
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT [FileId]
           ,[ChunkNum]
           ,[ChunkSize]
		   ,[ChunkContent]
	FROM [dbo].[FileChunks]
	WHERE [FileId] = @FileId

END

GO
