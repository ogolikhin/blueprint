USE [AdminStorage]
GO

/****** Object:  StoredProcedure [dbo].[GetConfigSettings]    Script Date: 11/2/2015 1:42:36 PM ******/
DROP PROCEDURE [dbo].[GetConfigSettings]
GO

/****** Object:  StoredProcedure [dbo].[GetConfigSettings]    Script Date: 11/2/2015 1:42:36 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetConfigSettings] 
	@AllowRestricted bit
AS
BEGIN
	SELECT [Key], [Value], [Group], IsRestricted FROM [dbo].ConfigSettings WHERE IsRestricted = @AllowRestricted OR @AllowRestricted = 1;
END

GO


