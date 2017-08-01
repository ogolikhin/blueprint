IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetUserPasswordRecoveryTokens]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetUserPasswordRecoveryTokens]
GO

CREATE PROCEDURE [AdminStore].[GetUserPasswordRecoveryTokens]
(
    @token as nvarchar(max)
)
AS
BEGIN
	SELECT b.[Login],b.[CreationTime],b.[RecoveryToken] FROM [AdminStore].[PasswordRecoveryTokens] a
	INNER JOIN [AdminStore].[PasswordRecoveryTokens] b
	ON a.[Login] = b.[Login]
	WHERE a.[RecoveryToken] = @token
	ORDER BY [CreationTime] DESC
END
GO 