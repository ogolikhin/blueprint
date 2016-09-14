/******************************************************************************************************************************
Name:			ValidateExpiryTime

Description: 
			
Change History:
Date			Name					Change

******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[ValidateExpiryTime]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [FileStore].[ValidateExpiryTime]
GO

CREATE FUNCTION [FileStore].[ValidateExpiryTime]
(
	--CurrentTime needs to be a parameter for InsertFileHead usage, to have stored time and expire time equal if set to expire now.	
	@currentTime AS datetime,
	@expiredTime AS datetime
)
RETURNS datetime
AS
BEGIN
	IF @expiredTime IS NOT NULL AND @expiredTime < @currentTime
	begin
		SET @expiredTime = @currentTime;
	end
	return @expiredTime;
END

GO