DECLARE @db_id int;
DECLARE @DBName AS nvarchar(1000);
DECLARE @StorageDBName AS nvarchar(1000);
DECLARE @sql AS nvarchar(max);
DECLARE @FileSize AS BIGINT; 
DECLARE @FileGrowth AS BIGINT; 

-- Set the name of the main Blueprint DB
SET @DBName = N'Blueprint'; -- REPLACE --

SET @StorageDBName = @DBName + N'_AdminStorage';
SET @FileSize=10;
SET @FileGrowth=128; 
--
IF db_id(@StorageDBName) IS NULL
BEGIN
	SET @sql = N'CREATE DATABASE [' + @StorageDBName + '];';
	EXEC(@sql);
END 

SET @db_id = DB_ID(@StorageDBName);
--PRINT @db_id;

--check data file size
SET @FileSize=(SELECT size * 8 / 1024 AS size_in_mb from sys.master_files where database_id=@db_id and type=0);
IF (@FileSize<100)
BEGIN
	SET @sql = N'ALTER DATABASE [' + @StorageDBName + '] MODIFY FILE ( NAME = N''' + @StorageDBName + ''', SIZE = 100MB)'
	EXEC(@sql);
END


SET @sql = N'ALTER DATABASE [' + @StorageDBName + '] MODIFY FILE ( NAME = N''' + @StorageDBName + ''',  MAXSIZE = UNLIMITED, FILEGROWTH = 10%)'
EXEC(@sql);

--check log file size
SET @FileSize=(SELECT size * 8 / 1024 AS size_in_mb from sys.master_files where database_id=@db_id and type=1); 
IF (@FileSize<10)
BEGIN
	SET @sql = N'ALTER DATABASE [' + @StorageDBName + '] MODIFY FILE ( NAME = N''' + @StorageDBName + '_log'', SIZE = 10MB)'
	EXEC(@sql);
END

SET @sql = N'ALTER DATABASE [' + @StorageDBName + '] MODIFY FILE (NAME = N''' + @StorageDBName + '_log'',  MAXSIZE = UNLIMITED, FILEGROWTH = 10%)'
EXEC(@sql);