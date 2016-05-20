/******************************************************************************************************************************
Name:			GetStatus

Description:    Returns the version of the database.
			
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetStatus]
GO

CREATE PROCEDURE [dbo].[GetStatus]
AS
BEGIN
       SELECT [SchemaVersion] FROM [dbo].[DbVersionInfo] WHERE [Id] = 1;       
END

GO