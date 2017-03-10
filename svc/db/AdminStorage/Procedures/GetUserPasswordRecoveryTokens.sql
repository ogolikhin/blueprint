IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserPasswordRecoveryTokens]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetUserPasswordRecoveryTokens]
GO

CREATE PROCEDURE [dbo].[GetUserPasswordRecoveryTokens]
(
    @token as nvarchar(max)
)
AS
BEGIN
	SELECT b.[Login],b.[CreationTime],b.[RecoveryToken] FROM [dbo].[PasswordRecoveryTokens] a
	INNER JOIN [dbo].[PasswordRecoveryTokens] b
	ON a.[Login] = [b.Login]
	WHERE a.[PasswordRecoveryTokens] = @token
	ORDER BY [CreationTime] DESC
END
GO 