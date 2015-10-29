/******************************************************************************************************************************
Name:			EnableFullText

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
	EXEC [FileStore].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO