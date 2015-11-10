/******************************************************************************************************************************
Name:			ApplicationLabels

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApplicationLabels]') AND type in (N'U'))
DROP TABLE [dbo].[ApplicationLabels]
GO

CREATE TABLE [dbo].[ApplicationLabels](
	[Key] [nvarchar](64) NOT NULL,
	[Locale] [nvarchar](32) NOT NULL,
	[Text] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_ApplicationLabels_Key_Locale] PRIMARY KEY CLUSTERED 
(
	[Key] ASC,
	[Locale] ASC
)) ON [PRIMARY]
GO
