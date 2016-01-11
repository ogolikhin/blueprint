/******************************************************************************************************************************
Name:			ValidateExpiryTime

Description: 
			
Change History:
Date			Name					Change

******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ValidateExpiryTime]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[ValidateExpiryTime]
GO

CREATE FUNCTION [dbo].[ValidateExpiryTime]
(
	@storedTime AS datetime,
	@expiredTime AS datetime
)
RETURNS datetime
AS
BEGIN
	IF @expiredTime < @storedTime
	begin
		SET @expiredTime = @storedTime;
	end
	return @expiredTime;
END

GO