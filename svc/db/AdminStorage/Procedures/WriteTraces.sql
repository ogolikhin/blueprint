/******************************************************************************************************************************
Name:			WriteTraces

Description: 
			
Change History:
Date			Name					Change
2015/12/17		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WriteTraces]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[WriteTraces]
GO

CREATE PROCEDURE [dbo].[WriteTraces]  
(
  @InsertTraces TracesType READONLY
)
AS
BEGIN
  INSERT INTO [Traces] (
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
		[ActivityId],
		[RelatedActivityId],
		[ProcessId],
		[ThreadId],
		[IpAddress],
		[Source],
		[MethodName],
		[FilePath],
		[LineNumber],
		[StackTrace]
	)
  SELECT * FROM @InsertTraces;
END

GO
