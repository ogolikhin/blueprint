
-- -----------------------------------------------------------------------------------------------
-- Migration 7.4.0.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([AdminStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0) 
	set noexec on
Print 'Migrating 7.4.0.0 ...'
-- -----------------------------------------------------------------------------------------------

-- -----------------------------------------------------------------------------
-- Modify [AdminStore].[ApplicationLabels] to have a primary key on [Key] and [Locale]
-- -----------------------------------------------------------------------------

IF  EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = 'PK_ApplicationLabels_ApplicationLabelId')
BEGIN

	-- Remove existing constraint
	ALTER TABLE [AdminStore].[ApplicationLabels] 
	DROP CONSTRAINT [PK_ApplicationLabels_ApplicationLabelId]

	-- Creating primary key on [ApplicationLabelId], [Key], [Locale] in table 'ApplicationLabels'
	ALTER TABLE [AdminStore].[ApplicationLabels]
	ADD CONSTRAINT [PK_ApplicationLabels] PRIMARY KEY NONCLUSTERED 
	(
		[Key], [Locale] ASC
	);
END

GO

-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	EXEC [AdminStore].[SetSchemaVersion] @value = N'7.4.0';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------
