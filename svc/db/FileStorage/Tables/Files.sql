/******************************************************************************************************************************
Name:			Files

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF__Files__FileId__117F9D94]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Files] DROP CONSTRAINT [DF__Files__FileId__117F9D94]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[FK_FileId]') AND type = 'F')
AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FileChunks]') AND type in (N'U'))
BEGIN
ALTER TABLE [dbo].[FileChunks] DROP CONSTRAINT [FK_FileId]
END
GO




IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Files]') AND type in (N'U'))
DROP TABLE [dbo].[Files]
GO

CREATE TABLE [dbo].[Files](
	[FileId] [uniqueidentifier] NOT NULL,
	[StoredTime] [datetime] NOT NULL,
	[ExpiredTime] [datetime],
	[FileName] [nvarchar](256) NOT NULL,
	[FileType] [nvarchar](128) NOT NULL,
	[ChunkCount] [int] NOT NULL,
	[FileSize] [bigint] NOT NULL,
 CONSTRAINT [PK_Files] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC
))
GO

ALTER TABLE [dbo].[Files] ADD  DEFAULT (newsequentialid()) FOR [FileId]
GO
