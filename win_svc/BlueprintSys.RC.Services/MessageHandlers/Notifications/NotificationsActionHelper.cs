using System;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Security;
using ServiceLibrary.Models.Email;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Notification.Templates;

namespace BlueprintSys.RC.Services.MessageHandlers.Notifications
{
    public class NotificationsActionHelper : MessageActionHandler
    {
        protected override async Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage, IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            var message = (NotificationMessage) actionMessage;
            var result = await SendNotificationEmail(tenant, message, (INotificationRepository) actionHandlerServiceRepository);
            Logger.Log($"Finished processing message with result: {result}", message, tenant, LogLevel.Debug);
            return await Task.FromResult(result == SendEmailResult.Success);
        }

        private async Task<SendEmailResult> SendNotificationEmail(TenantInformation tenant, NotificationMessage message, INotificationRepository repository)
        {
            Logger.Log($"Handling started for user ID {message.UserId} with message {message.ToJSON()}", message, tenant, LogLevel.Debug);
            //It should be responsibility of notification handler to recieve information about email settings
            //Maybe this information can be cached for tenant ids???
            var emailSettings = await new EmailSettingsRetriever().GetEmailSettings(repository);
            if (emailSettings == null)
            {
                Logger.Log($"No email settings provided for tenant {tenant.TenantId}", message, tenant, LogLevel.Error);
                return await Task.FromResult(SendEmailResult.Error);
            }

            //Get the logo
            byte[] logoImageArray = new LogoDataProvider().GetLogo();
            string logoImageSrc = null;
            if (logoImageArray != null)
            {
                logoImageSrc = GetImageSrc(logoImageArray, DiscussionEmail.LogoImageAttachmentContentId);
            }

            var notificationEmail = new NotificationEmail(
                message.ProjectId,
                message.ProjectName,
                message.ArtifactId,
                message.ArtifactName,
                message.ArtifactUrl,
                message.Message,
                message.Header,
                logoImageSrc,
                message.BlueprintUrl);

            var emailMessage = new Message
            {
                Subject = message.Subject,
                FromDisplayName = message.From,
                To = message.To.ToArray(),
                From = emailSettings.SenderEmailAddress,
                IsBodyHtml = true,
                Body = new NotificationEmailContent(notificationEmail).TransformText()
            };

            if (logoImageArray != null)
            {
                emailMessage.DiscussionEmail = new DiscussionEmail
                {
                    LogoImageSrc = logoImageSrc,
                    LogoImageAttachmentArray = logoImageArray
                };
            }

            var smtpClientConfiguration = new SMTPClientConfiguration
            {
                Authenticated = emailSettings.Authenticated,
                EnableSsl = emailSettings.EnableSSL,
                HostName = emailSettings.HostName,
                // Bug 61312: Password must be decrypted before saving in configuration
                Password = SystemEncryptions.DecryptFromSilverlight(emailSettings.Password),
                Port = emailSettings.Port,
                UserName = emailSettings.UserName
            };

            if (string.IsNullOrWhiteSpace(smtpClientConfiguration.HostName))
            {
                Logger.Log("No Host Name", message, tenant, LogLevel.Error);
                return await Task.FromResult(SendEmailResult.Error);
            }

            Logger.Log("Sending Email", message, tenant, LogLevel.Debug);
            InternalSendEmail(smtpClientConfiguration, emailMessage, repository);
            Logger.Log("Email Sent", message, tenant, LogLevel.Debug);
            return await Task.FromResult(SendEmailResult.Success);
        }

        private static string GetImageSrc(byte[] imageArray, string attachmentContentId)
        {
            if (imageArray == null)
            {
                return null;
            }
            if (attachmentContentId == null)
            {
                return "data:image/png;base64," + Convert.ToBase64String(imageArray);
            }
            return "cid:" + attachmentContentId;
        }

        //for unit testing
        private void InternalSendEmail(SMTPClientConfiguration smtpClientConfiguration, Message emailMessage, INotificationRepository repository)
        {
            repository.SendEmail(smtpClientConfiguration, emailMessage);
        }
    }
}
