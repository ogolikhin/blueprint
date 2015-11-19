﻿/******************************************************************************************************************************
Name:			Files

Description: 
			
Change History:
Date			Name					Change
2015/11/19		Albert Wong				Added FileChunks table
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FileChunks]') AND type in (N'U'))
DROP TABLE [dbo].[FileChunks]
GO

CREATE TABLE [dbo].[FileChunks](
	[FileId] [uniqueidentifier] NOT NULL,
	[ChunkNumber] [int] NOT NULL,
	[ChunkSize] [int] NOT NULL,
	[ChunkContent ] [varbinary](max) NULL,
 CONSTRAINT [PK_FileChunks] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC,
	[ChunkNumber] ASC
),
 CONSTRAINT [FK_FileId]
 FOREIGN KEY ([FileId])
    REFERENCES [dbo].[Files]
        ([FileId])
    ON DELETE CASCADE ON UPDATE CASCADE

) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO