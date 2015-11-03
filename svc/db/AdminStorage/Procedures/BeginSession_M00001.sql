SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BeginSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[BeginSession]
GO

/******************************************************************************************************************************
Name:			BeginSession

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

CREATE PROCEDURE [dbo].[BeginSession] 
(
	@UserId int,
	@BeginTime datetime,
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
		INSERT [dbo].[Sessions](UserId, SessionId, BeginTime) VALUES(@UserId, @NewSessionId, @BeginTime);
	END
	ELSE
	BEGIN
		UPDATE [dbo].[Sessions] SET SessionId = @NewSessionId, BeginTime = @BeginTime, EndTime = NULL where UserId = @UserId;
	END
	COMMIT TRANSACTION;
END

GO
SET QUOTED_IDENTIFIER ON 
GO
--SET ANSI_NULLS ON 
--GO
--GRANT  EXECUTE  ON [dbo].[BeginSession]  TO [Blueprint]

--GO
