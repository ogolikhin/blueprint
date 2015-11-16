/******************************************************************************************************************************
Name:			GetStatus

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetStatus]
GO

CREATE PROCEDURE [dbo].[GetStatus]
AS
BEGIN
       SELECT TOP 1 COUNT(*) FROM [dbo].[Files];       
END

GO