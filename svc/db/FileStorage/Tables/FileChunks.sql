/******************************************************************************************************************************
Name:			Files

Description: 
			
Change History:
Date			Name					Change
2015/11/19		Albert Wong				Added FileChunks table
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[FileChunks]') AND type in (N'U'))
DROP TABLE [FileStore].[FileChunks]
GO

CREATE TABLE [FileStore].[FileChunks](
	[FileId] [uniqueidentifier] NOT NULL,
	[ChunkNum] [int] NOT NULL,
	[ChunkSize] [int] NOT NULL,
	[ChunkContent] [varbinary](max) NULL,
 CONSTRAINT [PK_FileChunks] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC,
	[ChunkNum] ASC
),
 CONSTRAINT [FK_FileId]
 FOREIGN KEY ([FileId])
    REFERENCES [FileStore].[Files]
        ([FileId])
    ON DELETE CASCADE ON UPDATE CASCADE

) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO