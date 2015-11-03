SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetApplicationLables]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetApplicationLables]
GO

/******************************************************************************************************************************
Name:			GetApplicationLables

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

CREATE PROCEDURE [dbo].[GetApplicationLables] 
	@Locale nvarchar(32)
AS
BEGIN
	SELECT [Key], [Text] FROM [dbo].ApplicationLabels WHERE Locale = @Locale;
END

GO
SET QUOTED_IDENTIFIER ON 
GO
--SET ANSI_NULLS ON 
--GO
--GRANT  EXECUTE  ON [dbo].[GetApplicationLables]  TO [Blueprint]

--GO
