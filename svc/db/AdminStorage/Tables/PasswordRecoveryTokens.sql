IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[PasswordRecoveryTokens]') AND type in (N'U'))
DROP TABLE [AdminStore].[PasswordRecoveryTokens]
GO

CREATE TABLE [AdminStore].[PasswordRecoveryTokens](
    [Login] [nvarchar](max),
    [CreationTime] [datetime] NOT NULL,
    [RecoveryToken] [uniqueidentifier] NOT NULL,

	 CONSTRAINT [PK_PasswordRecoveryTokens] PRIMARY KEY CLUSTERED 
(
	[RecoveryToken] ASC
)) ON [PRIMARY]
GO
