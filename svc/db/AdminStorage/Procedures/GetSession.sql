USE [AdminStorage]
GO

/****** Object:  StoredProcedure [dbo].[GetSession]    Script Date: 11/2/2015 1:42:52 PM ******/
DROP PROCEDURE [dbo].[GetSession]
GO

/****** Object:  StoredProcedure [dbo].[GetSession]    Script Date: 11/2/2015 1:42:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetSession] 
(
	@SessionId uniqueidentifier
)
AS
BEGIN
	SELECT UserId, SessionId, BeginTime, EndTime from [dbo].[Sessions] where SessionId = @SessionId;
END

GO


