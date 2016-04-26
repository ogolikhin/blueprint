
-- --------------------------------------------------
-- Set the DB
-- --------------------------------------------------
SET QUOTED_IDENTIFIER ON;
GO
USE [Blueprint_ArtifactStorage]; -- REPLACE --
GO
SET NOCOUNT ON;
GO
-- --------------------------------------------------

DECLARE @db_name AS nvarchar(128) = DB_NAME();
DECLARE @sql AS nvarchar(max);

SET @sql = N'ALTER DATABASE [' + @db_name + N'] SET COMPATIBILITY_LEVEL = 110'; -- SQL Server 2012
EXEC(@sql);