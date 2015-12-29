/******************************************************************************************************************************
Name:			Traces

Description: 
			
Change History:
Date			Name					Change
2015/12/17		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Traces]') AND type in (N'U'))
DROP TABLE [dbo].[Traces]
GO

CREATE TABLE [dbo].[Traces](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[IpAddress] [nvarchar](45),
	[Source] [nvarchar](100),
	[FormattedMessage] [nvarchar](4000) NULL,
	[MethodName] [nvarchar](100),
	[FilePath] [nvarchar](1000),
	[LineNumber] [int],
	[StackTrace] [nvarchar](4000),
	[InstanceName] [nvarchar](1000) NOT NULL,
	[ProviderId] [uniqueidentifier] NOT NULL,
	[ProviderName] [nvarchar](500) NOT NULL,
	[EventId] [int] NOT NULL,
	[EventKeywords] [bigint] NOT NULL,
	[Level] [int] NOT NULL,
	[Opcode] [int] NOT NULL,
	[Task] [int] NOT NULL,
	[Timestamp] [datetimeoffset](7) NOT NULL,
	[Version] [int] NOT NULL,
	[Payload] [xml] NULL,
	[ActivityId] [uniqueidentifier],
	[RelatedActivityId] [uniqueidentifier],
	[ProcessId] [int],
	[ThreadId] [int],
	 CONSTRAINT [PK_Traces] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)) ON [PRIMARY]
GO