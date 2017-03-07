
-- -----------------------------------------------------------------------------------------------
-- Migration 8.1.0.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([dbo].[IsSchemaVersionLessOrEqual](N'8.1.0') <> 0) 
	set noexec on
Print 'Migrating 8.1.0.0 ...'
-- -----------------------------------------------------------------------------------------------

IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PasswordRecoveryTokens]') AND type in (N'U'))
    CREATE TABLE [dbo].[PasswordRecoveryTokens](
        [Login] [nvarchar](max),
        [CreationTime] [datetime] NOT NULL,
        [RecoveryToken] [uniqueidentifier] NOT NULL,

	     CONSTRAINT [PK_PasswordRecoveryTokens] PRIMARY KEY CLUSTERED 
    (
	    [RecoveryToken] ASC
    )) ON [PRIMARY]
GO


-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([dbo].[IsSchemaVersionLessOrEqual](N'8.1.0') <> 0)
	EXEC [dbo].[SetSchemaVersion] @value = N'8.1.0';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------
