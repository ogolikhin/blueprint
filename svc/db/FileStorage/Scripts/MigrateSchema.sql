
-- Migrate tables to the FileStore schema
IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[Files]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[FileStore].[Files]', 'U') IS NULL)
	ALTER SCHEMA [FileStore] TRANSFER [dbo].[Files];
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[FileChunks]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[FileStore].[FileChunks]', 'U') IS NULL)
	ALTER SCHEMA [FileStore] TRANSFER [dbo].[FileChunks];
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[MigrationLog]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[FileStore].[MigrationLog]', 'U') IS NULL)
	ALTER SCHEMA [FileStore] TRANSFER [dbo].[MigrationLog];
GO

IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[DeleteFile]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[FileStore].[DeleteFile]', 'U') IS NULL)
	ALTER SCHEMA [FileStore] TRANSFER [dbo].[DeleteFile];
GO
