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