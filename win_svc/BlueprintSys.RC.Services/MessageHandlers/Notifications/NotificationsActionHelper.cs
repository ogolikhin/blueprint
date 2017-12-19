using System;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Security;
using ServiceLibrary.Models.Email;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Notification.Templates;

namespace BlueprintSys.RC.Services.MessageHandlers.Notifications
{
    public class NotificationsActionHelper : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {
            var message = (NotificationMessage) actionMessage;
            var result = await SendNotificationEmail(tenant, message, (INotificationRepository) baseRepository);
            Logger.Log($"Finished processing message with result: {result}", message, tenant);
            return await Task.FromResult(result == SendEmailResult.Success);
        }

        private async Task<SendEmailResult> SendNotificationEmail(TenantInformation tenant, NotificationMessage message, INotificationRepository repository)
        {
            Logger.Log($"Handling started for user ID {message.UserId} with message {message.ToJSON()}", message, tenant);

            Logger.Log("Getting email settings", message, tenant);
            var emailSettings = await repository.GetEmailSettings();
            if (emailSettings == null)
            {
                Logger.Log($"Failed to send email because no email settings were provided for tenant {tenant.TenantId}", message, tenant, LogLevel.Error);
                return await Task.FromResult(SendEmailResult.Error);
            }
            Logger.Log($"Email settings found. Host Name: {emailSettings.HostName}. Sender Email Address: {emailSettings.SenderEmailAddress}. User Name: {emailSettings.UserName}", message, tenant);

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
                Logger.Log("Sending email failed because no host name was found. Check your email settings", message, tenant, LogLevel.Error);
                return await Task.FromResult(SendEmailResult.Error);
            }

            Logger.Log($"Sending Email to {string.Join(",", emailMessage.To)}", message, tenant);
            repository.SendEmail(smtpClientConfiguration, emailMessage);
            Logger.Log("Email Sent Successfully", message, tenant);
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
    }
}
