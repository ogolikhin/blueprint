USE [AdminStorage]
GO

/****** Object:  StoredProcedure [dbo].[EndSession]    Script Date: 11/2/2015 1:38:36 PM ******/
DROP PROCEDURE [dbo].[EndSession]
GO

/****** Object:  StoredProcedure [dbo].[EndSession]    Script Date: 11/2/2015 1:38:36 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[EndSession] 
(
	@SessionId uniqueidentifier,
	@EndTime datetime
)
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
	BEGIN TRANSACTION;
	UPDATE [dbo].[Sessions] SET BeginTime = NULL, EndTime = @EndTime where SessionId = @SessionId;
	COMMIT TRANSACTION;
END

GO


