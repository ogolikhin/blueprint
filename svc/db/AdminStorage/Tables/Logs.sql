/******************************************************************************************************************************
Name:			Logs

Description: 
			
Change History:
Date			Name					Change
2015/12/17		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[Logs]') AND type in (N'U'))
DROP TABLE [AdminStore].[Logs]
GO

CREATE TABLE [AdminStore].[Logs](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[IpAddress] [nvarchar](45),
	[Source] [nvarchar](100),
	[FormattedMessage] [nvarchar](4000) NULL,
	[OccurredAt] [datetimeoffset](7) NOT NULL,
	[UserName] [nvarchar](max),
	[SessionId] [nvarchar](40),
	[ActionName] [nvarchar](200),
	[CorrelationId] [uniqueidentifier],
	[Duration] [float],
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
	 CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)) ON [PRIMARY]
GO