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

INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Yes', 'en-US', N'Yes')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_No', 'en-US', N'No')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Ok', 'en-US', N'OK')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Cancel', 'en-US', N'Cancel')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Open', 'en-US', N'Open')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_DialogTitle_Alert', 'en-US', N'Warning')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_DialogTitle_Confirmation', 'en-US', N'Confirmation')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_LoadingMsg', 'en-US', N'Loading ...')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_PrerequisiteMsg_JavaScript', 'en-US', N'You must enable JavaScript to use this application')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_Name', 'en-US', N'Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_SignedInAs', 'en-US', N'Signed in as')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_Welcome', 'en-US', N'Welcome')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Tooltip_App_Header_Help', 'en-US', N'Help')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Tooltip_App_Header_Logout', 'en-US', N'Log out')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Tooltip_Artifact_Header_LegacyArtifact', 'en-US', N'Legacy artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Project', 'en-US', N'Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Project_Open', 'en-US', N'Open Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Project_Name', 'en-US', N'Name')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Project_Description', 'en-US', N'Description')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Project', 'en-US', N'Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Open_Project', 'en-US', N'Open Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Open_Recent_Projects', 'en-US', N'Open Recent Projects')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Close_Project', 'en-US', N'Close Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Close_All_Projects', 'en-US', N'Close All Projects')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Save_All', 'en-US', N'Save All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Discard_All', 'en-US', N'Discard All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Publish_All', 'en-US', N'Publish All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Refresh_All', 'en-US', N'Refresh All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Delete', 'en-US', N'Delete')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_Discussions', 'en-US', N'Discussions')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_Properties', 'en-US', N'Properties')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_Attachments', 'en-US', N'Attachments')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_Relationships', 'en-US', N'Relationships')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_History', 'en-US', N'History')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_Reviews', 'en-US', N'Reviews')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Filter_SortByLatest', 'en-US', N'Sort by latest')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Filter_SortByEarliest', 'en-US', N'Sort by earliest')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_History_Version', 'en-US', N'Version')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_History_Draft', 'en-US', N'Draft')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_History_Deleted', 'en-US', N'Deleted')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_History_No_Artifact', 'en-US', N'No history available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Add_New', 'en-US', N'Add New')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Add_Attachment', 'en-US', N'Attachment')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Add_Ref_Doc', 'en-US', N'Doc Reference')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_All', 'en-US', N'All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Attachments', 'en-US', N'Attachments')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Ref_Docs', 'en-US', N'Doc References')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Uploaded_By', 'en-US', N'Uploaded by')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_No_Attachments', 'en-US', N'No doc references/attachments available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_No_Attachments_Only', 'en-US', N'No attachments available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_No_Documents_Only', 'en-US', N'No doc references available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Download', 'en-US', N'Download')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Delete', 'en-US', N'Delete')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Download_No_Attachment', 'en-US', N'There''s no attachment available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Introduction_Message', 'en-US', N'You may collaborate with others using this discussion panel to add comments to any selected artifact.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Start_Discussion', 'en-US', N'+ Start a new discussion')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_New_Discussion', 'en-US', N'+ New comment')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_No_Artifact', 'en-US', N'The item you selected has no information to display or you do not have access to view it.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_New_Post_Button_Text', 'en-US', N'Post comment')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_New_Cancel_Button_Text', 'en-US', N'Cancel')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_New_Comment_Place_Holder', 'en-US', N'Add a new comment...')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_New_Reply_Post_Button_Text', 'en-US', N'Post reply')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_New_Reply_Cancel_Button_Text', 'en-US', N'Cancel')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_New_Reply_Comment_Place_Holder', 'en-US', N'Add a reply...')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Show_Replies', 'en-US', N'Show replies')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Hide_Replis', 'en-US', N'Hide replies')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_View_Comments_Tooltip', 'en-US', N'View comments')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Delete_Comment_Tooltip', 'en-US', N'Delete comment')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Edit_Comment_Tooltip', 'en-US', N'Edit comment')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Reply_Tooltip', 'en-US', N'Reply to the comment')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Delete_Reply_Tooltip', 'en-US', N'Delete reply')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Edit_Reply_Tooltip', 'en-US', N'Edit reply')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_All', 'en-US', N'All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Traces', 'en-US', N'Traces')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Association', 'en-US', N'Other')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Collapse', 'en-US', N'Hide details')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Expand', 'en-US', N'Show details')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_No_Relationships', 'en-US', N'No relationships available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_No_Traces', 'en-US', N'No traces available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_No_Association', 'en-US', N'No other relationships available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Subheading_Association', 'en-US', N'Association')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Subheading_Actor_Inherits', 'en-US', N'Actor Inherits')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Subheading_Document_Reference', 'en-US', N'Document Reference')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Unauthorized', 'en-US', N'Unauthorized')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Selected', 'en-US', N'Selected:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Glossary_Term', 'en-US', N'Term')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Glossary_Definition', 'en-US', N'Definition')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Glossary_Empty', 'en-US', N'No terms have been defined.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_CannotGetUser', 'en-US', N'Cannot get current user')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_IncorrectRequestId', 'en-US', N'Wrong request id. Please click ''Retry'' in Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LoginFailed', 'en-US', N'Login Failed')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_SamlContinueSessionWithOriginalUser', 'en-US', N'To continue your session, please login with the same user that the session was started with.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseVerificationFailed', 'en-US', N'Cannot verify license')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_SessionTokenRetrievalFailed', 'en-US', N'Cannot get Session Token')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseNotFound_Verbose', 'en-US', N'No licenses found or Blueprint is using an invalid server license. Please contact your Blueprint Administrator')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseLimitReached', 'en-US', N'The maximum concurrent license limit has been reached. Please contact your Blueprint Administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_FederatedAuthFailed', 'en-US', N'There is a problem with federated authentication. Please contact your administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_FederatedFallbackDisabled', 'en-US', N'Please log in with your corporate credentials.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Username', 'en-US', N'Username')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Password', 'en-US', N'Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ForgotPassword', 'en-US', N'Forgot Password?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePasswordButton', 'en-US', N'Change Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_UpdatePasswordButton', 'en-US', N'Update Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_LoginButton', 'en-US', N'Log In')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_SamlLink', 'en-US', N'Log in with corporate credentials')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_GoBackToLogin', 'en-US', N'Go back to login')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ResetPassword', 'en-US', N'Reset Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Retry', 'en-US', N'Retry')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_LoginPrompt', 'en-US', N'Log in with username and password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePassword_CurrentPassword', 'en-US', N'Current Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePassword_NewPassword', 'en-US', N'New Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePassword_ConfirmPassword', 'en-US', N'Confirm New Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_BlueprintCopyRight', 'en-US', N'Blueprint Software Systems Inc. All rights reserved')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Version', 'en-US', N'Version:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_CredentialsCannotBeEmpty', 'en-US', N'Username and password cannot be empty.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_DuplicateSession_Verbose', 'en-US', N'This user is already logged into Blueprint in another browser/session.<br><br>Do you want to override the previous session?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterCredentials', 'en-US', N'Please enter your username and password.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterUsername', 'en-US', N'Please enter your username')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordHasExpired_ChangePasswordPrompt', 'en-US', N'Your password has expired. Please update it below.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterSamlCredentials_Verbose', 'en-US', N'Please authenticate using your corporate credentials in the popup window that has opened. If you do not see the window, please ensure your popup blocker is disabled and then click the Retry button.<br><br>You will be automatically logged in after you are authenticated.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_CredentialsInvalid', 'en-US', N'Ensure your username and password are correct.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_ADUserNotInDB', 'en-US', N'You do not have a Blueprint account. <br>Please contact your administrator for access.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_AccountDisabled', 'en-US', N'Your account has been disabled. <br>Please contact your administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordHasExpired', 'en-US', N'Your password has expired.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterCurrentPassword', 'en-US', N'Ensure your current password is correct.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_CurrentPasswordCannotBeEmpty', 'en-US', N'Please enter your current password.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordChangedSuccessfully', 'en-US', N'Your password has been updated.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordConfirmMismatch', 'en-US', N'Ensure your new password and confirmation match.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordMaxLength', 'en-US', N'Your new password cannot be longer than 128 characters.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordMinLength', 'en-US', N'Your new password must be at least 8 characters long.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordCannotBeEmpty', 'en-US', N'New password cannot be empty.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordSameAsOld', 'en-US', N'Ensure your new password is different <br>from the current one.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordCriteria', 'en-US', N'Your new password must contain at least one number, <br>uppercase letter, and non-alphanumeric character.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_Timeout', 'en-US', N'​Your session has expired or was overridden.<br>Please log in to continue.​')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Input_Required', 'en-US', N'Required')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Folder_NotFound', 'en-US', N'Couldn''t find specified project or folder')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Project_NotFound', 'en-US', N'Couldn''t find the project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Project_MetaDataNotFound', 'en-US', N'Couldn''t find the project meta data')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_NotFound', 'en-US', N'Couldn''t find the artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ArtifactType_NotFound', 'en-US', N'Couldn''t find the artifact type')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Name', 'en-US', N'Name')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Type', 'en-US', N'Type')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_CreatedBy', 'en-US', N'Created by')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_CreatedOn', 'en-US', N'Created on')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_LastEditBy', 'en-US', N'Last edited by')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_LastEditOn', 'en-US', N'Last edited on')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Description', 'en-US', N'Description')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Project', 'en-US', N'Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Collections', 'en-US', N'Collections')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Diagram_OldFormat_Message', 'en-US', N'This diagram is stored in an old format that is incompatible with this version. To display the diagram, please open it in Silverlight Main Experience, make a modification, and publish it.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Details_FieldNameError', 'en-US', N'The field name isn''t specified')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Datepicker_Today', 'en-US', N'Today')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Datepicker_Clear', 'en-US', N'Clear')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Datepicker_Done', 'en-US', N'Close')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Cannot_Be_Empty', 'en-US', N'This value is required.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Wrong_Format', 'en-US', N'Please check the format of the value.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Decimal_Places', 'en-US', N'There is a maximum number of decimal places:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Value_Must_Be', 'en-US', N'The value must be')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Date_Must_Be', 'en-US', N'The date must be')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Suffix_Or_Later', 'en-US', N'or later.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Suffix_Or_Earlier', 'en-US', N'or earlier.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Suffix_Or_Greater', 'en-US', N'or greater.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Suffix_Or_Less', 'en-US', N'or less.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Project_NoProjectsAvailable', 'en-US', N'Either no projects are available or you do not have the required permissions to access them.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Message1', 'en-US', N'An error ocurred while loading the page.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Message2', 'en-US', N'Please try again.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Message3', 'en-US', N'If the problem persists, contact your Blueprint administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Label', 'en-US', N'Error')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Shape_Limit_Exceeded_Initial_Load', 'en-US', N'The Process has {0} shapes. It exceeds the maximum of {1} shapes and cannot be edited. Please refactor it and move more detailed user tasks to included Processes.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Shape_Limit_Exceeded', 'en-US', N'The shape cannot be added. The Process will exceed the maximum {0} shapes. Please refactor it and move more detailed user tasks to included Processes.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Eighty_Percent_of_Shape_Limit_Reached', 'en-US', N'The Process now has {0} of the maximum {1} shapes. Please consider refactoring it to move more detailed user tasks to included Processes.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Yes', 'fr-CA', N'Oui')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_No', 'fr-CA', N'Non')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Ok', 'fr-CA', N'D''accord')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Cancel', 'fr-CA', N'Annuler')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_DialogTitle_Confirmation', 'fr-CA', N'Confirmation')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_LoadingMsg', 'fr-CA', N'Chargement …')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_PrerequisiteMsg_JavaScript', 'fr-CA', N'Vous devez activer JavaScript pour utiliser cette application')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_Name', 'fr-CA', N'Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_SignedInAs', 'fr-CA', N'Signé en tant')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_Welcome', 'fr-CA', N'Bienvenue')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_CannotGetUser', 'fr-CA', N'Vous ne pouvez pas obtenir l''utilisateur actuel')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_IncorrectRequestId', 'fr-CA', N'Mauvais demande id . S''il vous plaît cliquer sur ''Retry'' dans Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LoginFailed', 'fr-CA', N'Échec de la connexion')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_SamlContinueSessionWithOriginalUser', 'fr-CA', N'Pour continuer votre session, s''il vous plaît vous connecter avec le même utilisateur que la session a commencé avec.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseVerificationFailed', 'fr-CA', N'Impossible de vérifier la licence')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_SessionTokenRetrievalFailed', 'fr-CA', N'Vous ne pouvez pas obtenir jeton de session')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseNotFound_Verbose', 'fr-CA', N'Aucune licence trouvé ou Blueprint utilise une licence de serveur non valide . S''il vous plaît contactez votre administrateur Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseLimitReached', 'fr-CA', N'La limite maximale de licence concurrente a été atteint. S''il vous plaît contactez votre administrateur Blueprint.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_FederatedAuthFailed', 'fr-CA', N'There is a problem with federated authentication. Please contact your administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Username', 'fr-CA', N'Nom d''utilisateur')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Password', 'fr-CA', N'Mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ForgotPassword', 'fr-CA', N'Mot de passe oublié?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePasswordButton', 'fr-CA', N'Changer le mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_UpdatePasswordButton', 'fr-CA', N'Update Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_LoginButton', 'fr-CA', N'S''identifier')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_SamlLink', 'fr-CA', N'Connectez-vous pour blueprint.toronto')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_GoBackToLogin', 'fr-CA', N'Retour à identifier')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ResetPassword', 'fr-CA', N'Réinitialiser le mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Retry', 'fr-CA', N'Recommencez')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_LoginPrompt', 'fr-CA', N'Connexion avec identifiant et mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePassword_CurrentPassword', 'fr-CA', N'Mot de passe actuel')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePassword_NewPassword', 'fr-CA', N'Nouveau mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePassword_ConfirmPassword', 'fr-CA', N'Confirmer le nouveau mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_BlueprintCopyRight', 'fr-CA', N'Blueprint Software Systems Inc. Tous droits réservés')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Version', 'fr-CA', N'Version:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_CredentialsCannotBeEmpty', 'fr-CA', N'Nom d''utilisateur et mot de passe ne peut pas être vide')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_DuplicateSession_Verbose', 'fr-CA', N'Cet utilisateur est déjà connecté à Blueprint dans un autre navigateur / session. <br><br>Voulez-vous remplacer la session précédente ?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterCredentials', 'fr-CA', N'Veuillez s''il vous plaît entrer votre nom d''utilisateur et votre mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterUsername', 'fr-CA', N'S''il vous plaît entrez votre nom d''utilisateur')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordHasExpired_ChangePasswordPrompt', 'fr-CA', N'Votre mot de passe a expiré. S''il vous plaît changer votre mot de passe ci-dessous.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterSamlCredentials_Verbose', 'fr-CA', N'S''il vous plaît authentifier en utilisant vos informations d''identification d''entreprise dans la fenêtre qui a ouvert . Si vous ne voyez pas la fenêtre , s''il vous plaît vous assurer que votre bloqueur de popup est désactivé , puis cliquez sur le bouton Retry. <br><br> Vous serez automatiquement connecté après que vous êtes authentifié .')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_CredentialsInvalid', 'fr-CA', N'S''il vous plaît entrer un nom d''utilisateur correct et mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_AccountDisabled', 'fr-CA', N'Votre compte a été désactivé. <br> S''il vous plaît contactez votre administrateur.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordHasExpired', 'fr-CA', N'Votre mot de passe a expiré.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterCurrentPassword', 'fr-CA', N'Please enter the correct current Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_CurrentPasswordCannotBeEmpty', 'fr-CA', N'Current Password cannot be empty')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordChangedSuccessfully', 'fr-CA', N'Password changed successfully')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordConfirmMismatch', 'fr-CA', N'Confirm password does not match new password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordMaxLength', 'fr-CA', N'New password must be at most 128 characters long')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordMinLength', 'fr-CA', N'New password must be at least 8 characters long')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordCannotBeEmpty', 'fr-CA', N'New password cannot be empty')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordSameAsOld', 'fr-CA', N'New password cannot be the same as the old one')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordCriteria', 'fr-CA', N'New password must contain at least one capital letter, number and symbol')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Input_Required', 'fr-CA', N'Obligatoire')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Message1', 'fr-CA', N'An error ocurred while loading the page.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Message2', 'fr-CA', N'Please try again.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Message3', 'fr-CA', N'If the problem persists, contact your Blueprint administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Label', 'fr-CA', N'Error')

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
	   AND [dbo].[ApplicationLabels].[Text] <> #tempAppLabels.[Text] COLLATE SQL_Latin1_General_CP437_BIN2

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
