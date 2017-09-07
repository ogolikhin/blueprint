/******************************************************************************************************************************
Name:			GetSession

Description: 
			
Change History:
Date			Name					Change
2015/11/17		Anton Trinkunas			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetUserSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetUserSession]
GO

CREATE PROCEDURE [AdminStore].[GetUserSession] 
(
	@UserId int
)
AS
BEGIN
	SELECT UserId, SessionId, BeginTime, EndTime, UserName, LicenseLevel, IsSso from [AdminStore].[Sessions] where UserId = @UserId;
END
GO 