
-- Migrate tables to the AdminStore schema
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[ApplicationLabels]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[AdminStore].[ApplicationLabels]', 'U') IS NULL)
	ALTER SCHEMA [AdminStore] TRANSFER [dbo].[ApplicationLabels];
GO

IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[ConfigSettings]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[AdminStore].[ConfigSettings]', 'U') IS NULL)
	ALTER SCHEMA [AdminStore] TRANSFER [dbo].[ConfigSettings];
GO

IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[Sessions]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[AdminStore].[Sessions]', 'U') IS NULL)
	ALTER SCHEMA [AdminStore] TRANSFER [dbo].[Sessions];
GO

IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[LicenseActivityDetails]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[AdminStore].[LicenseActivityDetails]', 'U') IS NULL)
	ALTER SCHEMA [AdminStore] TRANSFER [dbo].[LicenseActivityDetails];
GO

IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[LicenseActivities]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[AdminStore].[LicenseActivities]', 'U') IS NULL)
	ALTER SCHEMA [AdminStore] TRANSFER [dbo].[LicenseActivities];
GO

IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[Logs]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[AdminStore].[Logs]', 'U') IS NULL)
	ALTER SCHEMA [AdminStore] TRANSFER [dbo].[Logs];
GO

IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[PasswordRecoveryTokens]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[AdminStore].[PasswordRecoveryTokens]', 'U') IS NULL)
	ALTER SCHEMA [AdminStore] TRANSFER [dbo].[PasswordRecoveryTokens];
GO
