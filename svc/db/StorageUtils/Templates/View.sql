SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[<View_Name>]'))
DROP VIEW [dbo].[<View_Name>]
GO


/******************************************************************************************************************************
Name:			<View_Name>

Description: 
			
Change History:
Date			Name					Change

******************************************************************************************************************************/

CREATE View dbo.[<View_Name>]
AS
Select 1

GO
SET QUOTED_IDENTIFIER ON 
GO
--SET ANSI_NULLS ON 
--GO
--GRANT REFERENCES, SELECT ON [dbo].[<View_Name>]  TO [Blueprint]
--GO
