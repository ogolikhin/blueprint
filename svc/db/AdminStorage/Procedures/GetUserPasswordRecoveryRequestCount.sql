
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserPasswordRecoveryRequestCount]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].GetUserPasswordRecoveryRequestCount
GO

CREATE PROCEDURE [dbo].GetUserPasswordRecoveryRequestCount
(
    @login as nvarchar(max)
)
AS
BEGIN
    SELECT COUNT([Login])
    FROM [dbo].[PasswordRecoveryTokens]
    WHERE [Login] = @login
    AND [CreationTime] > DATEADD(d,-1,CURRENT_TIMESTAMP)
END
GO 

