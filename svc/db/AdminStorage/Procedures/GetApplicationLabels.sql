USE [AdminStorage]
GO

/****** Object:  StoredProcedure [dbo].[GetApplicationLables]    Script Date: 11/2/2015 1:42:13 PM ******/
DROP PROCEDURE [dbo].[GetApplicationLables]
GO

/****** Object:  StoredProcedure [dbo].[GetApplicationLables]    Script Date: 11/2/2015 1:42:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetApplicationLables] 
	@Locale nvarchar(32)
AS
BEGIN
	SELECT [Key], [Text] FROM [dbo].ApplicationLabels WHERE Locale = @Locale;
END

GO


