/******************************************************************************************************************************
Name:			GetStatus

Description:	Returns the version of the database.

******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetStatus]
GO

CREATE PROCEDURE [AdminStore].[GetStatus] 
AS
BEGIN
	SELECT [SchemaVersion] FROM [AdminStore].[DbVersionInfo] WHERE [Id] = 1;
END
GO 

