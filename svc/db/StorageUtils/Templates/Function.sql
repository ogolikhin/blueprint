SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[<Function_Name>]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[<Function_Name>]
GO


/******************************************************************************************************************************
Name:			<Function_Name>

Description: 
			
Change History:
Date			Name					Change

******************************************************************************************************************************/

CREATE Function dbo.[<Function_Name>]
(
)
RETURNS TABLE
AS
Return
(	
	Select 1
)

GO
SET QUOTED_IDENTIFIER ON 
GO
--SET ANSI_NULLS ON 
--GO
--GRANT  EXECUTE  ON [dbo].[<Function_Name>]  TO [Blueprint]
--GO
