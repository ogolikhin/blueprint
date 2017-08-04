/******************************************************************************************************************************
Name:			Sessions

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[Sessions]') AND type in (N'U'))
DROP TABLE [AdminStore].[Sessions]
GO

CREATE TABLE [AdminStore].[Sessions](
	[UserId] [int] NOT NULL,
	[SessionId] [uniqueidentifier] NOT NULL,
	[BeginTime] [datetime] NULL,
	[EndTime] [datetime] NOT NULL,
	[UserName] [nvarchar](max) NOT NULL,
	[LicenseLevel] [int] NOT NULL,
	[IsSso] [bit] NOT NULL
 CONSTRAINT [PK_Sessions] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)) ON [PRIMARY]
GO
