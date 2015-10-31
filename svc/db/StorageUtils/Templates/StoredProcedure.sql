SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[<Procedure_Name>]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[<Procedure_Name>]
GO

/******************************************************************************************************************************
Name:			<Procedure_Name>

Description: 
			
Change History:
Date			Name					Change

******************************************************************************************************************************/

CREATE PROCEDURE dbo.[<Procedure_Name>]
(
	-- Add the parameters for the stored procedure here
	<@param1 int>
)

AS
Begin

-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
SET NOCOUNT ON

End

GO
SET QUOTED_IDENTIFIER ON 
GO
--SET ANSI_NULLS ON 
--GO
--GRANT  EXECUTE  ON [dbo].[<Procedure_Name>]  TO [Blueprint]

--GO
