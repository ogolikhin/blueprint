/******************************************************************************************************************************
Name:			Files

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[FileStore].[DF__Files__FileId__117F9D94]') AND type = 'D')
BEGIN
ALTER TABLE [FileStore].[Files] DROP CONSTRAINT [DF__Files__FileId__117F9D94]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[FileStore].[FK_FileId]') AND type = 'F')
AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[FileChunks]') AND type in (N'U'))
BEGIN
ALTER TABLE [FileStore].[FileChunks] DROP CONSTRAINT [FK_FileId]
END
GO




IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[Files]') AND type in (N'U'))
DROP TABLE [FileStore].[Files]
GO

CREATE TABLE [FileStore].[Files](
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

ALTER TABLE [FileStore].[Files] ADD  DEFAULT (newsequentialid()) FOR [FileId]
GO
