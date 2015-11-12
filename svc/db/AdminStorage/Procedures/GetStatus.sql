/******************************************************************************************************************************
Name:			GetStatus

Description: //TODO: 
			
Change History:
Date			Name					Change

******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetStatus]
GO

CREATE PROCEDURE [dbo].[GetStatus] 
AS
BEGIN
	SELECT COUNT(*) from [dbo].[Sessions];
END
GO 

