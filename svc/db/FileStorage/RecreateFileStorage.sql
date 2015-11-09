SET QUOTED_IDENTIFIER OFF;
GO

USE [master]
GO


IF EXISTS(select * from sys.databases where name='FileStorage') 
DROP DATABASE [FileStorage]
GO

CREATE DATABASE [FileStorage]
GO
ALTER DATABASE [FileStorage] MODIFY FILE
( NAME = N'FileStorage', SIZE = 50MB , MAXSIZE = UNLIMITED, FILEGROWTH = 10MB )
GO
ALTER DATABASE [FileStorage] MODIFY FILE
( NAME = N'FileStorage_log', SIZE = 5MB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
