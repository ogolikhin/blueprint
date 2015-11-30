DECLARE @DBName AS nvarchar(50);
DECLARE @sql AS nvarchar(4000);
DECLARE @ParmDefinition nvarchar(4000) = N'@LoginIdentity varchar(100)';

---------------------------------------------------------------------------------------
-- Define the databases
---------------------------------------------------------------------------------------
DECLARE @DB_Blueprint			as nvarchar(50)	= N'Raptor'
DECLARE @DB_AdminStorage		as nvarchar(50)	= N'Blueprint_AdminStorage'
DECLARE @DB_ArtifactStorage		as nvarchar(50)	= N'Blueprint_ArtifactStorage'
DECLARE @DB_FileStorage			as nvarchar(50)	= N'Blueprint_FileStorage'
---------------------------------------------------------------------------------------

---------------------------------------------------------------------------------------
-- Define the Windows Application Pool Login
---------------------------------------------------------------------------------------
DECLARE @LoginIdentity AS nvarchar(100) = 'IIS APPPOOL\Blueprint'
---------------------------------------------------------------------------------------

-- Delete login
SET @SQL = N'IF  EXISTS (SELECT * FROM sys.server_principals WHERE name = @LoginIdentity) DROP LOGIN [' + @LoginIdentity + N'];';
EXECUTE sp_executesql @sql, @ParmDefinition, @LoginIdentity=@LoginIdentity

-- Create login
SET @SQL = N'CREATE LOGIN [' + @LoginIdentity + N'] FROM WINDOWS'
EXECUTE sp_executesql @sql

---------------------------------------------------------------------------------------
SET @DBName = @DB_Blueprint;
---------------------------------------------------------------------------------------

-- Delete database user
SET @SQL = N'USE [' + @DBName + N']; IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = @LoginIdentity) DROP USER [' + @LoginIdentity + N'];';
EXECUTE sp_executesql @sql, @ParmDefinition, @LoginIdentity=@LoginIdentity

-- Create database user
SET @SQL = N'USE [' + @DBName + N']; CREATE USER [' + @LoginIdentity + N'] FOR LOGIN [' + @LoginIdentity + N'] WITH DEFAULT_SCHEMA=[dbo]'
EXECUTE sp_executesql @sql

-- Add roles to database user
SET @SQL = N'USE [' + @DBName + N']; ALTER ROLE [db_blueprint_reader] ADD MEMBER [' + @LoginIdentity + N'];
USE [' + @DBName + N']; ALTER ROLE [db_blueprint_writer] ADD MEMBER [' + @LoginIdentity + N'];
USE [' + @DBName + N']; ALTER ROLE [db_blueprint_executor] ADD MEMBER [' + @LoginIdentity + N']'
EXECUTE sp_executesql @sql

---------------------------------------------------------------------------------------
SET @DBName = @DB_AdminStorage;
---------------------------------------------------------------------------------------

-- Delete database user
SET @SQL = N'USE [' + @DBName + N']; IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = @LoginIdentity) DROP USER [' + @LoginIdentity + N'];';
EXECUTE sp_executesql @sql, @ParmDefinition, @LoginIdentity=@LoginIdentity

-- Create database user
SET @SQL = N'USE [' + @DBName + N']; CREATE USER [' + @LoginIdentity + N'] FOR LOGIN [' + @LoginIdentity + N'] WITH DEFAULT_SCHEMA=[dbo]'
EXECUTE sp_executesql @sql

-- Add roles to database user
SET @SQL = N'USE [' + @DBName + N']; ALTER ROLE [db_blueprint_reader] ADD MEMBER [' + @LoginIdentity + N'];
USE [' + @DBName + N']; ALTER ROLE [db_blueprint_writer] ADD MEMBER [' + @LoginIdentity + N'];
USE [' + @DBName + N']; ALTER ROLE [db_blueprint_executor] ADD MEMBER [' + @LoginIdentity + N']'
EXECUTE sp_executesql @sql

---------------------------------------------------------------------------------------
SET @DBName = @DB_ArtifactStorage;
---------------------------------------------------------------------------------------

-- Delete database user
SET @SQL = N'USE [' + @DBName + N']; IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = @LoginIdentity) DROP USER [' + @LoginIdentity + N'];';
EXECUTE sp_executesql @sql, @ParmDefinition, @LoginIdentity=@LoginIdentity

-- Create database user
SET @SQL = N'USE [' + @DBName + N']; CREATE USER [' + @LoginIdentity + N'] FOR LOGIN [' + @LoginIdentity + N'] WITH DEFAULT_SCHEMA=[dbo]'
EXECUTE sp_executesql @sql

-- Add roles to database user
SET @SQL = N'USE [' + @DBName + N']; ALTER ROLE [db_blueprint_reader] ADD MEMBER [' + @LoginIdentity + N'];
USE [' + @DBName + N']; ALTER ROLE [db_blueprint_writer] ADD MEMBER [' + @LoginIdentity + N'];
USE [' + @DBName + N']; ALTER ROLE [db_blueprint_executor] ADD MEMBER [' + @LoginIdentity + N']'
EXECUTE sp_executesql @sql

---------------------------------------------------------------------------------------
SET @DBName = @DB_FileStorage;
---------------------------------------------------------------------------------------

-- Delete database user
SET @SQL = N'USE [' + @DBName + N']; IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = @LoginIdentity) DROP USER [' + @LoginIdentity + N'];';
EXECUTE sp_executesql @sql, @ParmDefinition, @LoginIdentity=@LoginIdentity

-- Create database user
SET @SQL = N'USE [' + @DBName + N']; CREATE USER [' + @LoginIdentity + N'] FOR LOGIN [' + @LoginIdentity + N'] WITH DEFAULT_SCHEMA=[dbo]'
EXECUTE sp_executesql @sql

-- Add roles to database user
SET @SQL = N'USE [' + @DBName + N']; ALTER ROLE [db_blueprint_reader] ADD MEMBER [' + @LoginIdentity + N'];
USE [' + @DBName + N']; ALTER ROLE [db_blueprint_writer] ADD MEMBER [' + @LoginIdentity + N'];
USE [' + @DBName + N']; ALTER ROLE [db_blueprint_executor] ADD MEMBER [' + @LoginIdentity + N']'
EXECUTE sp_executesql @sql
