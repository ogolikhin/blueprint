USE [AdminStorage]
GO

/****** Object:  StoredProcedure [dbo].[BeginSession]    Script Date: 11/2/2015 1:36:26 PM ******/
DROP PROCEDURE [dbo].[BeginSession]
GO

/****** Object:  StoredProcedure [dbo].[BeginSession]    Script Date: 11/2/2015 1:36:26 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

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


