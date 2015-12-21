/******************************************************************************************************************************
Name:			LicenseActivities

Description: 
			
Change History:
Date			Name					Change
2015/12/16		Glen Stone				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LicenseActivities]') AND type in (N'U'))
DROP TABLE [dbo].[LicenseActivities]
GO

CREATE TABLE [dbo].[LicenseActivities](
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

ALTER TABLE [dbo].[LicenseActivityDetails]  WITH CHECK ADD  CONSTRAINT [FK_LicenseActivityDetails_LicenseActivities] FOREIGN KEY([LicenseActivityId])
REFERENCES [dbo].[LicenseActivities] ([LicenseActivityId])
GO

ALTER TABLE [dbo].[LicenseActivityDetails] CHECK CONSTRAINT [FK_LicenseActivityDetails_LicenseActivities]
GO
