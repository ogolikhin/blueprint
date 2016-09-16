/******************************************************************************************************************************
Name:			DbVersionInfo

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[DbVersionInfo]') AND type in (N'U'))
DROP TABLE [FileStore].[DbVersionInfo]
GO

CREATE TABLE [FileStore].[DbVersionInfo](
	[Id] [int] NOT NULL,
	[SchemaVersion] [nvarchar](32) NULL,
 CONSTRAINT [PK_DbVersionInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)) ON [PRIMARY]
GO