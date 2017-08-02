/******************************************************************************************************************************
Name:			WriteLogs

Description: 
			
Change History:
Date			Name					Change
2015/12/17		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[WriteLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[WriteLogs]
GO

CREATE PROCEDURE [AdminStore].[WriteLogs]  
(
  @InsertLogs LogsType READONLY
)
AS
BEGIN
  INSERT INTO [AdminStore].[Logs] (
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
  SELECT * FROM @InsertLogs;
END

GO
