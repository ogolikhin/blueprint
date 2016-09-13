/******************************************************************************************************************************
Name:			[MakeFilePermanent]

Description: 
			
Change History:
Date			Name					Change
2016/09/13		Alexander Utkin		    Initial
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MakeFilePermanent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MakeFilePermanent]
GO

CREATE PROCEDURE [dbo].[MakeFilePermanent]
( 
    @FileId uniqueidentifier
)
AS
BEGIN

	UPDATE 
		[dbo].[Files]
    SET
		[ExpiredTime] = NULL
	WHERE 
		[FileId] = @FileId;

	SELECT @@ROWCOUNT
END

GO
