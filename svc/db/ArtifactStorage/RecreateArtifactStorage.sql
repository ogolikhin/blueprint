SET QUOTED_IDENTIFIER OFF;
GO

USE [master]
GO


IF EXISTS(select * from sys.databases where name='ArtifactStorage') 
DROP DATABASE [ArtifactStorage]
GO

CREATE DATABASE [ArtifactStorage]
GO
ALTER DATABASE [ArtifactStorage] MODIFY FILE
( NAME = N'ArtifactStorage', SIZE = 50MB , MAXSIZE = UNLIMITED, FILEGROWTH = 10MB )
GO
ALTER DATABASE [ArtifactStorage] MODIFY FILE
( NAME = N'ArtifactStorage_log', SIZE = 5MB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO