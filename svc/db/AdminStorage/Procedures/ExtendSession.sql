IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExtendSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExtendSession]
GO

CREATE PROCEDURE [dbo].[ExtendSession] 
(
	@SessionId uniqueidentifier,
	@EndTime datetime
)
AS
BEGIN
	UPDATE [dbo].[Sessions] SET EndTime = @EndTime
	OUTPUT Inserted.[UserId], Inserted.[SessionId], Inserted.[BeginTime], Inserted.[EndTime], Inserted.[UserName], Inserted.[LicenseLevel], Inserted.[IsSso]
	WHERE SessionId = @SessionId AND EndTime < @EndTime;
END
GO
