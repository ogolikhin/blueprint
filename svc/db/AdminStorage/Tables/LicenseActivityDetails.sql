/******************************************************************************************************************************
Name:			LicenseActivityDetails

Description: 
			
Change History:
Date			Name					Change
2015/12/16		Glen Stone				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[LicenseActivityDetails]') AND type in (N'U'))
DROP TABLE [AdminStore].[LicenseActivityDetails]
GO

CREATE TABLE [AdminStore].[LicenseActivityDetails](
	[LicenseActivityId] [int] NOT NULL,
	[LicenseType] [int] NOT NULL,
	[Count] [int] NOT NULL,
 CONSTRAINT [PK_LicenseActivityDetails] PRIMARY KEY CLUSTERED 
(
	[LicenseActivityId] ASC,
	[LicenseType] ASC
)) ON [PRIMARY]
