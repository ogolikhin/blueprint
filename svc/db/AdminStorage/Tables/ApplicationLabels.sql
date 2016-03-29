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
	[ApplicationLabelId] [int] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](128) NOT NULL,
	[Locale] [nvarchar](32) NOT NULL,
	[Text] [nvarchar](512) NOT NULL,
 CONSTRAINT [PK_ApplicationLabels_ApplicationLabelId_Locale] PRIMARY KEY CLUSTERED 
(
	[ApplicationLabelId] ASC
)) ON [PRIMARY]
GO
