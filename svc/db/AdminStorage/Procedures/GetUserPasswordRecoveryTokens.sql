IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserPasswordRecoveryTokens]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetUserPasswordRecoveryTokens]
GO

CREATE PROCEDURE [dbo].[GetUserPasswordRecoveryTokens]
(
    @login as nvarchar(max)
)
AS
BEGIN
	SELECT [Login],[CreationTime],[RecoveryToken] FROM [dbo].[PasswordRecoveryTokens]
	WHERE [Login] = @login
	ORDER BY [CreationTime] DESC
END
GO 