-- Drop all functions associated with dbo schema
IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[IsSchemaVersionLessOrEqual]', 'FN') IS NOT NULL)
	DROP FUNCTION [dbo].[IsSchemaVersionLessOrEqual];
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ValidateExpiryTime]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[ValidateExpiryTime]
GO

-- Drop all procedures associated with dbo schema
IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteFile]
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteFileChunk]
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetStatus]
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HeadFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[HeadFile]
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertFileChunk]
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertFileHead]
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReadChunkContent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ReadChunkContent]
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReadFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ReadFileChunk]
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReadFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ReadFileHead]
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetSchemaVersion]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SetSchemaVersion]
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateFileHead]
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteFile]
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetSchemaVersion]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SetSchemaVersion]
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateFileHead]
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MakeFilePermanent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MakeFilePermanent]
GO
