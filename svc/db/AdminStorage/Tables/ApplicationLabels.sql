﻿/******************************************************************************************************************************
Name:			ApplicationLabels

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
2016/09/29		Areag Osman				Extends character limit for Key & Text columns, adds index for table
2016/10/06		Areag Osman				Creates a primary key on [Key], [Locale]
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[ApplicationLabels]') AND type in (N'U'))
DROP TABLE [AdminStore].[ApplicationLabels]
GO

	CREATE TABLE [AdminStore].[ApplicationLabels](
		[ApplicationLabelId] [int] IDENTITY(1,1) NOT NULL,
		[Key] [nvarchar](128) NOT NULL,
		[Locale] [nvarchar](32) NOT NULL,
		[Text] [nvarchar](512) NOT NULL,

		CONSTRAINT [PK_ApplicationLabels] PRIMARY KEY NONCLUSTERED 
		(
			[Key], [Locale] ASC
		)
	) ON [PRIMARY]
GO

IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_ApplicationLabels_Key_Locale')
	DROP INDEX IX_ApplicationLabels_Key_Locale on [AdminStore].[ApplicationLabels]
GO

CREATE NONCLUSTERED INDEX IX_ApplicationLabels_Key_Locale on [AdminStore].[ApplicationLabels] 
(
	[Key] ASC,
	[Locale] ASC
)
GO
