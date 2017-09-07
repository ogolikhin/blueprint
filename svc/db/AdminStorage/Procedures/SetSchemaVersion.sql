/******************************************************************************************************************************
Name:			SetSchemaVersion

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[SetSchemaVersion]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[SetSchemaVersion]
GO

CREATE PROCEDURE [AdminStore].[SetSchemaVersion]
(
	@value AS nvarchar(max)
)
AS
PRINT 'Setting Schema Version to ' + @value;
-- Integrity check
DECLARE @value1 AS int = CAST(PARSENAME(@value, 1) AS int);
DECLARE @value2 AS int = CAST(PARSENAME(@value, 2) AS int);
DECLARE @value3 AS int = CAST(PARSENAME(@value, 3) AS int);
DECLARE @value4 AS int = CAST(PARSENAME(@value, 4) AS int);

IF EXISTS (SELECT * FROM [AdminStore].[DbVersionInfo])
	BEGIN 
		UPDATE [AdminStore].[DbVersionInfo] SET [SchemaVersion] = @value FROM [AdminStore].[DbVersionInfo];
	END
ELSE
	BEGIN 
		INSERT INTO [AdminStore].[DbVersionInfo] SELECT 1, @value;
	END 

GO
