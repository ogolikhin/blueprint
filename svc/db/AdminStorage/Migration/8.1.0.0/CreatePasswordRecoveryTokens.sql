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
