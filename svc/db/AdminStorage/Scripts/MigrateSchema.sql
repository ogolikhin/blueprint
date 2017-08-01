
-- Create the AdminStore Schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'AdminStore')
EXEC sys.sp_executesql N'CREATE SCHEMA [AdminStore]'
GO

-- Migrate tables to the AdminStore schema
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DbVersionInfo]') AND type in (N'U'))
ALTER SCHEMA AdminStore TRANSFER [dbo].[DbVersionInfo]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApplicationLabels]') AND type in (N'U'))
ALTER SCHEMA AdminStore TRANSFER [dbo].[ApplicationLabels]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConfigSettings]') AND type in (N'U'))
ALTER SCHEMA AdminStore TRANSFER [dbo].[ConfigSettings]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Sessions]') AND type in (N'U'))
ALTER SCHEMA AdminStore TRANSFER [dbo].[Sessions]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LicenseActivityDetails]') AND type in (N'U'))
ALTER SCHEMA AdminStore TRANSFER [dbo].[LicenseActivityDetails]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LicenseActivities]') AND type in (N'U'))
ALTER SCHEMA AdminStore TRANSFER [dbo].[LicenseActivities]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND type in (N'U'))
ALTER SCHEMA AdminStore TRANSFER [dbo].[Logs]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PasswordRecoveryTokens]') AND type in (N'U'))
ALTER SCHEMA AdminStore TRANSFER [dbo].[PasswordRecoveryTokens]
GO
