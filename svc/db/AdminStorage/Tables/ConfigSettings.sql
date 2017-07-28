/******************************************************************************************************************************
Name:			ConfigSettings

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[ConfigSettings]') AND type in (N'U'))
DROP TABLE [AdminStore].[ConfigSettings]
GO

CREATE TABLE [AdminStore].[ConfigSettings](
	[Key] [nvarchar](64) NOT NULL,
	[Value] [nvarchar](128) NOT NULL,
	[Group] [nvarchar](128) NOT NULL,
	[IsRestricted] [bit] NOT NULL,
 CONSTRAINT [PK_ConfigSettings] PRIMARY KEY CLUSTERED 
(
	[Key] ASC
)) ON [PRIMARY]
GO
