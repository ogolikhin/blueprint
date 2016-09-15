
-- Create the FileStore Schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'FileStore')
EXEC sys.sp_executesql N'CREATE SCHEMA [FileStore]'
GO
