IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[ApplicationLabels]') AND type in (N'U'))
DROP TABLE [AdminStore].[ApplicationLabels]
GO
