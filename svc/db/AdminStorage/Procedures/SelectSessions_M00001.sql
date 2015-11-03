SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SelectSessions]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SelectSessions]
GO

/******************************************************************************************************************************
Name:			SelectSessions

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

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
SET QUOTED_IDENTIFIER ON 
GO
--SET ANSI_NULLS ON 
--GO
--GRANT  EXECUTE  ON [dbo].[SelectSessions]  TO [Blueprint]

--GO
