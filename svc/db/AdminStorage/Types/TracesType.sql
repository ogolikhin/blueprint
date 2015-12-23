﻿/******************************************************************************************************************************
Name:			TracesType

Description: 
			
Change History:
Date			Name					Change
2015/12/17		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WriteTraces]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[WriteTraces]
GO

IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TracesType' AND ss.name = N'dbo')
DROP TYPE [dbo].[TracesType]
GO

CREATE TYPE TracesType AS TABLE
(
	[InstanceName] [nvarchar](1000),
	[ProviderId] [uniqueidentifier],
	[ProviderName] [nvarchar](500),
	[EventId] [int],
	[EventKeywords] [bigint],
	[Level] [int],
	[Opcode] [int],
	[Task] [int],
	[Timestamp] [datetimeoffset](7),
	[Version] [int],
	[FormattedMessage] [nvarchar](4000),
	[Payload] [xml],
	[ActivityId] [uniqueidentifier], 
	[RelatedActivityId] [uniqueidentifier],
	[ProcessId] [int],
	[ThreadId] [int],
	[IpAddress] [nvarchar](45),
	[Source] [nvarchar](100),
	[MethodName] [nvarchar](100),
	[FilePath] [nvarchar](1000),
	[LineNumber] [int],
	[StackTrace] [nvarchar](4000)
);
GO