/******************************************************************************************************************************
Name:			LicenseActivities

Description: 
			
Change History:
Date			Name					Change
2015/12/16		Glen Stone				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[LicenseActivities]') AND type in (N'U'))
DROP TABLE [AdminStore].[LicenseActivities]
GO

CREATE TABLE [AdminStore].[LicenseActivities](
	[LicenseActivityId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[UserLicenseType] [int] NOT NULL,
	[TransactionType] [int] NOT NULL,
	[ActionType] [int] NOT NULL,
	[ConsumerType] [int] NOT NULL,
	[TimeStamp] [datetime] NOT NULL,
 CONSTRAINT [PK_LicenseActivities] PRIMARY KEY CLUSTERED 
(
	[LicenseActivityId] ASC
)) ON [PRIMARY]
GO

ALTER TABLE [AdminStore].[LicenseActivityDetails]  WITH CHECK ADD  CONSTRAINT [FK_LicenseActivityDetails_LicenseActivities] FOREIGN KEY([LicenseActivityId])
REFERENCES [AdminStore].[LicenseActivities] ([LicenseActivityId])
GO

ALTER TABLE [AdminStore].[LicenseActivityDetails] CHECK CONSTRAINT [FK_LicenseActivityDetails_LicenseActivities]
GO
