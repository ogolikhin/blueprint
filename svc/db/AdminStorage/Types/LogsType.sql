/******************************************************************************************************************************
Name:			LogsType

Description: 
			
Change History:
Date			Name					Change
2015/12/17		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[WriteLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[WriteLogs]
GO

IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'LogsType' AND ss.name = N'AdminStore')
DROP TYPE [AdminStore].[LogsType]
GO

CREATE TYPE LogsType AS TABLE
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
	[IpAddress] [nvarchar](45),
	[Source] [nvarchar](100),
	[UserName] [nvarchar](Max),
	[SessionId] [nvarchar](40),
	[OccurredAt] [datetimeoffset](7) NOT NULL,
	[ActionName] [nvarchar](200),
	[CorrelationId] [uniqueidentifier],
	[Duration] [float]
);
GO