/******************************************************************************************************************************
Name:			GetApplicationLabels

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetApplicationLabels]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetApplicationLabels]
GO

CREATE PROCEDURE [AdminStore].[GetApplicationLabels] 
	@Locale nvarchar(32)
AS
BEGIN
	SELECT [Key], [Text] FROM [AdminStore].ApplicationLabels WHERE Locale = @Locale;
END
GO