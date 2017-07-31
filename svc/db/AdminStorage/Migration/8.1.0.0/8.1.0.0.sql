
-- -----------------------------------------------------------------------------------------------
-- Migration 8.1.0.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.1.0') <> 0) 
	set noexec on
Print 'Migrating 8.1.0.0 ...'
-- -----------------------------------------------------------------------------------------------

IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[PasswordRecoveryTokens]') AND type in (N'U'))
    CREATE TABLE [AdminStore].[PasswordRecoveryTokens](
        [Login] [nvarchar](max),
        [CreationTime] [datetime] NOT NULL,
        [RecoveryToken] [uniqueidentifier] NOT NULL,

	     CONSTRAINT [PK_PasswordRecoveryTokens] PRIMARY KEY CLUSTERED 
    (
	    [RecoveryToken] ASC
    )) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[ApplicationLabels]') AND type in (N'U'))
DROP TABLE [AdminStore].[ApplicationLabels]
GO


-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.1.0') <> 0)
	EXEC [AdminStore].[SetSchemaVersion] @value = N'8.1.0';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------
