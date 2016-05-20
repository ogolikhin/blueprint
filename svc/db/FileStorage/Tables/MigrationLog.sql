/******************************************************************************************************************************
Name:			MigrationLog

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Michael Talis			Initial Version
******************************************************************************************************************************/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MigrationLog]') AND type in (N'U'))
BEGIN
DROP TABLE [dbo].[MigrationLog]
END;

CREATE TABLE [dbo].[MigrationLog](
	[FileId] [uniqueidentifier] NOT NULL,
	[FileSize] [bigint] NULL,
	[TransferStatus] [int] NULL,
	[Message] [nvarchar](256) NULL,
	[StoredTime] [datetime] NULL,
 CONSTRAINT [PK_MigrationLog] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
