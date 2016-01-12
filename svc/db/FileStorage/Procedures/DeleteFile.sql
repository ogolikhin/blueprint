/******************************************************************************************************************************
Name:			DeleteFile

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteFile]
GO

CREATE PROCEDURE [dbo].[DeleteFile]
(
	@FileId uniqueidentifier,
	@ExpiredTime datetime
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON
	
	DECLARE @CurrentTime datetime;
	SELECT @CurrentTime = GETUTCDATE();
	SET @ExpiredTime = [dbo].[ValidateExpiryTime](@CurrentTime, @ExpiredTime);

	SET NOCOUNT ON

    UPDATE [dbo].[Files] SET ExpiredTime = @ExpiredTime
    WHERE [FileId] = @FileId

	SELECT @@ROWCOUNT
END

GO