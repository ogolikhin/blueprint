DECLARE @db_id int;
DECLARE @DBName AS nvarchar(1000);
DECLARE @StorageDBName AS nvarchar(1000);
DECLARE @sql AS nvarchar(max);

-- Set the name of the main Blueprint DB
SET @DBName = N'Blueprint'; -- REPLACE --
SET @StorageDBName = @DBName + N'_ArtifactStorage';
--

SET @db_id = DB_ID(@StorageDBName);

IF @db_id is not null
BEGIN
	SET @sql = N'DROP DATABASE [' + @StorageDBName + '];';
	EXEC(@sql);
END  

SET @sql = N'CREATE DATABASE [' + @StorageDBName + '];';
EXEC(@sql);
SET @sql = N'ALTER DATABASE [' + @StorageDBName + '] MODIFY FILE
( NAME = N''' + @StorageDBName + ''', SIZE = 50MB , MAXSIZE = UNLIMITED, FILEGROWTH = 10MB )'
EXEC(@sql);
SET @sql = N'ALTER DATABASE [' + @StorageDBName + '] MODIFY FILE
( NAME = N''' + @StorageDBName + '_log'', SIZE = 5MB , MAXSIZE = 2048GB , FILEGROWTH = 10%)'
EXEC(@sql);
