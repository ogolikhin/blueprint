
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetUserPasswordRecoveryToken]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].SetUserPasswordRecoveryToken
GO

CREATE PROCEDURE [dbo].SetUserPasswordRecoveryToken 
(
    @login as nvarchar(max)
)
AS
BEGIN
    INSERT INTO [dbo].[PasswordRecoveryTokens]
    ([Login],[CreationTime],[RecoveryToken])
    VALUES (@login, CURRENT_TIMESTAMP, NEWID())
END
GO 

