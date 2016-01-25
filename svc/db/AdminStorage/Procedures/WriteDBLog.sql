IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WriteDBLog]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[WriteDBLog]
GO

CREATE PROCEDURE [dbo].[WriteDBLog]  
(
  @Message nvarchar(4000)
)
AS
BEGIN
  DECLARE @occurredAt DateTimeOffset = SYSDATETIMEOFFSET()

  -- Get the IP Address
  -- If it does not exist as in the case of Shared Memory use the server name
  DECLARE @ipAddress nvarchar(45)
  SELECT @ipAddress=COALESCE(cn.local_net_address, @@SERVERNAME) FROM sys.dm_exec_connections cn WHERE cn.session_id = @@SPID

  INSERT INTO [Logs] (
		[InstanceName],
		[ProviderId],
		[ProviderName],
		[EventId],
		[EventKeywords],
		[Level],
		[Opcode],
		[Task],
		[Timestamp],
		[Version],
		[FormattedMessage],
		[Payload],
		[IpAddress],
		[Source],
		[UserName],
		[SessionId],
		[OccurredAt],
		[ActionName],
		[CorrelationId],
		[Duration]
	)
	VALUES (
		N'',
		N'00000000-0000-0000-0000-000000000000',
		N'',
		999,
		0,
		5,
		0,
		0,
		@occurredAt,
		0,
		@Message,
		N'<Payload />',
		@ipAddress,
		N'Database',
		N'',
		N'',
		@occurredAt,
		N'',
		N'00000000-0000-0000-0000-000000000000',
		0
	)
END

GO
