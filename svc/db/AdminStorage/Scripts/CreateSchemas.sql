
-- Create the AdminStore Schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'AdminStore')
EXEC sys.sp_executesql N'CREATE SCHEMA [AdminStore]'
GO
