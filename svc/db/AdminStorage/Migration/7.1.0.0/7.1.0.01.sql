
-- --------------------------------------------------
-- Migration 7.1.0.0
-- --------------------------------------------------
SET QUOTED_IDENTIFIER ON;
GO
USE [Blueprint_AdminStorage]; -- REPLACE --
GO
SET NOCOUNT ON;
GO
--IF NOT ([dbo].[IsSchemaVersionLessOrEqual](N'7.0.1') <> 0) 
--	set noexec on 

--Print 'Migrating 7.1.0.0 ...'
-- --------------------------------------------------
-- Temporary solution to populate ApplicationLabels Datatable
-- File ..\..\Scripts\ApplicationLabels.txt needs to be updated


DELETE FROM [dbo].[ApplicationLabels] 
GO

BULK
	INSERT [dbo].[ApplicationLabels] 
	FROM 'C:\Projects\blueprint\svc\db\AdminStorage\Migration\7.1.0.0\..\..\Scripts\ApplicationLabels.txt'
	WITH
(
FIELDTERMINATOR = ',',
ROWTERMINATOR = '\n'
)
GO

Print 'Completed'

-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
--IF ([dbo].[IsSchemaVersionLessOrEqual](N'7.0.1') <> 0)
--	EXEC [dbo].[SetSchemaVersion] @value = N'7.0.1';
GO
set noexec off
-- --------------------------------------------------