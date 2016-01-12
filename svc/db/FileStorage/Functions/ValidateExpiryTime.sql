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
	@expiredTime AS datetime,
	@fallbackTime AS datetime
)
RETURNS datetime
AS
BEGIN
	IF @expiredTime IS NOT NULL AND @expiredTime < @storedTime
	begin
		SET @expiredTime = @fallbackTime;
	end
	return @expiredTime;
END

GO