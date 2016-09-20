/******************************************************************************************************************************
Name:			GetStatus

Description:    Returns the version of the database.
			
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[GetStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[GetStatus]
GO

CREATE PROCEDURE [FileStore].[GetStatus]
AS
BEGIN
       SELECT [SchemaVersion] FROM [FileStore].[DbVersionInfo] WHERE [Id] = 1;       
END

GO