USE [AdminStorage]
GO

/****** Object:  StoredProcedure [dbo].[SelectSessions]    Script Date: 11/2/2015 1:43:13 PM ******/
DROP PROCEDURE [dbo].[SelectSessions]
GO

/****** Object:  StoredProcedure [dbo].[SelectSessions]    Script Date: 11/2/2015 1:43:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SelectSessions] 
(
	@ps int,
	@pn int
)
AS
BEGIN
	WITH SessionsRN AS
	(
		SELECT ROW_NUMBER() OVER(ORDER BY BeginTime DESC) AS RN, UserId, SessionId, BeginTime, EndTime FROM [dbo].[Sessions]
	)
	SELECT * FROM SessionsRN
	WHERE RN BETWEEN(@pn - 1)*@ps + 1 AND @pn * @ps
	ORDER BY BeginTime DESC;
END

GO


