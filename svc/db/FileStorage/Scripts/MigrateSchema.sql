
-- Create the FileStore Schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'FileStore')
EXEC sys.sp_executesql N'CREATE SCHEMA [FileStore]'
GO

-- Migrate tables to the FileStore schema
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DbVersionInfo]') AND type in (N'U'))
ALTER SCHEMA FileStore TRANSFER [dbo].[DbVersionInfo]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Files]') AND type in (N'U'))
ALTER SCHEMA FileStore TRANSFER [dbo].[Files]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FileChunks]') AND type in (N'U'))
ALTER SCHEMA FileStore TRANSFER [dbo].[FileChunks]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MigrationLog]') AND type in (N'U'))
ALTER SCHEMA FileStore TRANSFER [dbo].[MigrationLog]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteFile]') AND type in (N'P'))
ALTER SCHEMA FileStore TRANSFER [dbo].[DeleteFile]
GO

