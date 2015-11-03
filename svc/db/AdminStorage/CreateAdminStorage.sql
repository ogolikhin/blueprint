SET QUOTED_IDENTIFIER OFF;
GO

USE [master]
GO


IF EXISTS(select * from sys.databases where name='AdminStorage') 
DROP DATABASE [AdminStorage]
GO

CREATE DATABASE [AdminStorage]
GO
ALTER DATABASE [AdminStorage] MODIFY FILE
( NAME = N'AdminStorage', SIZE = 50MB , MAXSIZE = UNLIMITED, FILEGROWTH = 10MB )
GO
ALTER DATABASE [AdminStorage] MODIFY FILE
( NAME = N'AdminStorage_log', SIZE = 5MB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO

