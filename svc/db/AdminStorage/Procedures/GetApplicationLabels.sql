/******************************************************************************************************************************
Name:			GetApplicationLables

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetApplicationLables]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetApplicationLables]
GO

CREATE PROCEDURE [dbo].[GetApplicationLables] 
	@Locale nvarchar(32)
AS
BEGIN
	SELECT [Key], [Text] FROM [dbo].ApplicationLabels WHERE Locale = @Locale;
END
GO

--GRANT  EXECUTE  ON [dbo].[GetApplicationLables]  TO [Blueprint]
--GO
