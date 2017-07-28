
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetUserPasswordRecoveryRequestCount]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].GetUserPasswordRecoveryRequestCount
GO

CREATE PROCEDURE [AdminStore].GetUserPasswordRecoveryRequestCount
(
    @login as nvarchar(max)
)
AS
BEGIN
    SELECT COUNT([Login])
    FROM [AdminStore].[PasswordRecoveryTokens]
    WHERE [Login] = @login
    AND [CreationTime] > DATEADD(d,-1,CURRENT_TIMESTAMP)
END
GO 

