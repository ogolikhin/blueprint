/******************************************************************************************************************************
Name:			DeleteExpiredFiles

Description:    Delete Expired Files.
			
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[DeleteExpiredFiles]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[DeleteExpiredFiles]
GO

CREATE PROCEDURE [FileStore].[DeleteExpiredFiles]
AS
BEGIN
	
	SET NOCOUNT ON;
	DELETE FROM [FileStore].[Files] WHERE [FileStore].[Files].ExpiredTime <= GETDATE();

END
GO
