/******************************************************************************************************************************
Name:			GetSession

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetSession]
GO

CREATE PROCEDURE [AdminStore].[GetSession] 
(
	@SessionId uniqueidentifier
)
AS
BEGIN
	SELECT UserId, SessionId, BeginTime, EndTime, UserName, LicenseLevel, IsSso from [AdminStore].[Sessions] where SessionId = @SessionId;
END
GO 