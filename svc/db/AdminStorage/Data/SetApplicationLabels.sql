﻿
-- -----------------------------------------------------------------------------------------------
-- Insert statements for Application Labels are Auto-Generated by T4 template file.
--
-- To add/edit/remove application labels from the [dbo].[ApplicationLabels] table, please update the
-- CSV file located at: '~\blueprint\svc\db\AdminStorage\Data\ApplicationLabels_en-US.csv'
-- 
-- DO NOT EDIT THESE INSERT STATEMENTS DIRECTLY AS YOUR CHANGES WILL BE OVERWRITTEN

CREATE TABLE #tempAppLabels (
	[Key] [nvarchar](128) NOT NULL,
	[Locale] [nvarchar](32) NOT NULL,
	[Text] [nvarchar](512) NOT NULL
)

INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_PrerequisiteMsg_JavaScript', 'en-US', 'You must enable JavaScript to use this application')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_Name', 'en-US', 'Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_SignedInAs', 'en-US', 'Signed in as')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_CannotGetUser', 'en-US', 'Cannot get current user')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LoginFailed', 'en-US', 'Login Failed')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseInvalid', 'en-US', 'Cannot verify license')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_SessionTokenFailed', 'en-US', 'Cannot get Session Token')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseInvalid_Verbose', 'en-US', 'No licenses found or Blueprint is using an invalid server license. Please contact your Blueprint administrator')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseLimitReached', 'en-US', 'The maximum concurrent license limit has been reached. Please contact your Blueprint Administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Username', 'en-US', 'Username')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Password', 'en-US', 'Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ForgotPassword', 'en-US', 'Forgot password?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_LoginButton', 'en-US', 'Login')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_SamlLink', 'en-US', 'Login to blueprint.toronto')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_GoBackToLogin', 'en-US', 'Go back to login')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ResetPassword', 'en-US', 'Reset password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_BlueprintCopyRight', 'en-US', 'Blueprint Software Systems Inc. All rights reserved')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Version', 'en-US', 'Version:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_DuplicateSession_Verbose', 'en-US', 'This user is already logged into Blueprint in another browser/session.<br><br>Do you want to override the previous session?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterCredentials', 'en-US', 'Please enter your Username and Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterUsername', 'en-US', 'Please enter your Username')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_InvalidCredentials', 'en-US', 'Please enter a correct Username and Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_AccountDisabled', 'en-US', 'Your account has been disabled. <br>Please contact your administrator.')

-- Add application label if [Key]/[Locale] combination does not exist
INSERT INTO [dbo].[ApplicationLabels] ([Key], [Locale], [Text])
SELECT #tempAppLabels.[Key], #tempAppLabels.[Locale], #tempAppLabels.[Text]
  FROM #tempAppLabels
 WHERE NOT EXISTS ( SELECT *
					  FROM [dbo].[ApplicationLabels] 
					 WHERE [dbo].[ApplicationLabels].[Key] = #tempAppLabels.[Key]
					   AND [dbo].[ApplicationLabels].[Locale] = #tempAppLabels.[Locale])

-- Update if [Key]/[Locale] combination exists, but text is different
UPDATE [dbo].[ApplicationLabels]
   SET [dbo].[ApplicationLabels].[Text] = #tempAppLabels.[Text]
  FROM [dbo].[ApplicationLabels]
  JOIN #tempAppLabels 
		ON [dbo].[ApplicationLabels].[Key] = #tempAppLabels.[Key]
	   AND [dbo].[ApplicationLabels].[Locale] = #tempAppLabels.[Locale]
	   AND [dbo].[ApplicationLabels].[Text] <> #tempAppLabels.[Text]

-- Delete if [Key]/[Locale] combination no longer exists
DELETE
  FROM [dbo].[ApplicationLabels]
 WHERE NOT EXISTS ( SELECT *
					  FROM #tempAppLabels 
					 WHERE #tempAppLabels.[Key] = [dbo].[ApplicationLabels].[Key]
					   AND #tempAppLabels.[Locale] = [dbo].[ApplicationLabels].[Locale])

DROP TABLE #tempAppLabels

-- End of Auto-Generation of SQL insert statements for Application Labels
-- -----------------------------------------------------------------------------------------------
