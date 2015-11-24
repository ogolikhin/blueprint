/******************************************************************************************************************************
Name:			BeginSession

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BeginSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[BeginSession]
GO

CREATE PROCEDURE [dbo].[BeginSession] 
(
	@UserId int,
	@BeginTime datetime,
	@UserName nvarchar(max),
	@LicenseLevel int,
	@IsSso bit = 0,
	@NewSessionId uniqueidentifier OUTPUT,
	@OldSessionId uniqueidentifier OUTPUT
)
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
	BEGIN TRANSACTION;
	SELECT @OldSessionId = SessionId from [dbo].[Sessions] where UserId = @UserId;
	SELECT @NewSessionId = NEWID();
	IF @OldSessionId IS NULL
	BEGIN
		INSERT [dbo].[Sessions](UserId, SessionId, BeginTime, UserName, LicenseLevel, IsSso) 
		VALUES(@UserId, @NewSessionId, @BeginTime, @UserName, @LicenseLevel, @IsSso);
	END
	ELSE
	BEGIN
		UPDATE [dbo].[Sessions] 
		SET SessionId = @NewSessionId, BeginTime = @BeginTime, EndTime = NULL, 
			UserName = @UserName, LicenseLevel = @LicenseLevel, IsSso = @IsSso 
		WHERE UserId = @UserId;
	END
	COMMIT TRANSACTION;
END
GO
