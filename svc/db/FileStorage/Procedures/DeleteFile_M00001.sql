SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteFile]
GO

/******************************************************************************************************************************
Name:			DeleteFile

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

CREATE PROCEDURE [dbo].[DeleteFile]
(
	@FileId uniqueidentifier,
	@DeletedFileId AS uniqueidentifier OUTPUT
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	DECLARE @op TABLE (ColGuid uniqueidentifier)
    DELETE FROM [dbo].[Files]
	OUTPUT DELETED.FileId INTO @op
    WHERE [FileId] = @FileId
	SELECT  @DeletedFileId = t.ColGuid FROM @op t
END

GO
SET QUOTED_IDENTIFIER ON 
GO
--SET ANSI_NULLS ON 
--GO
--GRANT  EXECUTE  ON [dbo].[DeleteFile]  TO [Blueprint]

--GO
