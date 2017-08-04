
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[SetUserPasswordRecoveryToken]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].SetUserPasswordRecoveryToken
GO

CREATE PROCEDURE [AdminStore].SetUserPasswordRecoveryToken 
(
    @login as nvarchar(max),
    @recoverytoken as uniqueidentifier
)
AS
BEGIN
    INSERT INTO [AdminStore].[PasswordRecoveryTokens]
    ([Login],[CreationTime],[RecoveryToken])
    VALUES (@login, CURRENT_TIMESTAMP, @recoverytoken)
END
GO 

