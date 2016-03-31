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
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_DialogTitle_Confirmation', 'en-US', N'Confirmation')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_LoadingMsg', 'en-US', N'Loading …')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_PrerequisiteMsg_JavaScript', 'en-US', N'You must enable JavaScript to use this application')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_Name', 'en-US', N'Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_SignedInAs', 'en-US', N'Signed in as')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_CannotGetUser', 'en-US', N'Cannot get current user')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_IncorrectRequestId', 'en-US', N'Wrong request id. Please click ''Retry'' in Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LoginFailed', 'en-US', N'Login Failed')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_SamlContinueSessionWithOriginalUser', 'en-US', N'To continue your session, please login with the same user that the session was started with.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseVerificationFailed', 'en-US', N'Cannot verify license')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_SessionTokenRetrievalFailed', 'en-US', N'Cannot get Session Token')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseNotFound_Verbose', 'en-US', N'No licenses found or Blueprint is using an invalid server license. Please contact your Blueprint administrator')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseLimitReached', 'en-US', N'The maximum concurrent license limit has been reached. Please contact your Blueprint Administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Username', 'en-US', N'Username')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Password', 'en-US', N'Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ForgotPassword', 'en-US', N'Forgot password?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePasswordButton', 'en-US', N'Change Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_LoginButton', 'en-US', N'Login')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_SamlLink', 'en-US', N'Login to blueprint.toronto')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_GoBackToLogin', 'en-US', N'Go back to login')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ResetPassword', 'en-US', N'Reset password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Retry', 'en-US', N'Retry')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_LoginPrompt', 'en-US', N'Login with Username and Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePassword_CurrentPassword', 'en-US', N'Current Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePassword_NewPassword', 'en-US', N'New Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePassword_ConfirmPassword', 'en-US', N'Confirm New Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_BlueprintCopyRight', 'en-US', N'Blueprint Software Systems Inc. All rights reserved')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Version', 'en-US', N'Version:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_CredentialsCannotBeEmpty', 'en-US', N'Username and Password cannot be empty')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_DuplicateSession_Verbose', 'en-US', N'This user is already logged into Blueprint in another browser/session.<br><br>Do you want to override the previous session?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterCredentials', 'en-US', N'Please enter your Username and Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterUsername', 'en-US', N'Please enter your Username')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordHasExpired_ChangePasswordPrompt', 'en-US', N'Your Password has expired. Please change your Password below.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterSamlCredentials_Verbose', 'en-US', N'Please authenticate using your corporate credentials in the popup window that has opened. If you do not see the window, please ensure your popup blocker is disabled and then click the Retry button.<br><br>You will be automatically logged in after you are authenticated.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_CredentialsInvalid', 'en-US', N'Please enter a correct Username and Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_AccountDisabled', 'en-US', N'Your account has been disabled. <br>Please contact your administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordHasExpired', 'en-US', N'Your Password has expired.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Input_Required', 'en-US', N'Required')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Yes', 'fr-CA', N'Oui')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_No', 'fr-CA', N'Non')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Ok', 'fr-CA', N'D''accord')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Cancel', 'fr-CA', N'Annuler')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_DialogTitle_Confirmation', 'fr-CA', N'Confirmation')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_LoadingMsg', 'fr-CA', N'Chargement …')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_PrerequisiteMsg_JavaScript', 'fr-CA', N'Vous devez activer JavaScript pour utiliser cette application')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_Name', 'fr-CA', N'Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_SignedInAs', 'fr-CA', N'Signé en tant')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_CannotGetUser', 'fr-CA', N'Vous ne pouvez pas obtenir l''utilisateur actuel')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_IncorrectRequestId', 'fr-CA', N'Mauvais demande id . S''il vous plaît cliquer sur ''Retry'' dans Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LoginFailed', 'fr-CA', N'Échec de la connexion')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_SamlContinueSessionWithOriginalUser', 'fr-CA', N'Pour continuer votre session, s''il vous plaît vous connecter avec le même utilisateur que la session a commencé avec.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseVerificationFailed', 'fr-CA', N'Impossible de vérifier la licence')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_SessionTokenRetrievalFailed', 'fr-CA', N'Vous ne pouvez pas obtenir jeton de session')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseNotFound_Verbose', 'fr-CA', N'Aucune licence trouvé ou Blueprint utilise une licence de serveur non valide . S''il vous plaît contactez votre administrateur Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseLimitReached', 'fr-CA', N'La limite maximale de licence concurrente a été atteint. S''il vous plaît contactez votre administrateur Blueprint.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Username', 'fr-CA', N'Nom d''utilisateur')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Password', 'fr-CA', N'Mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ForgotPassword', 'fr-CA', N'Mot de passe oublié?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePasswordButton', 'fr-CA', N'Changer le mot de passe')
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
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Input_Required', 'fr-CA', N'Obligatoire')

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
