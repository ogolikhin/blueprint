using System.Linq;
using System.Threading.Tasks;
using ActionHandlerService.Helpers;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Security;
using ServiceLibrary.Models.Email;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Notification.Templates;

namespace ActionHandlerService.MessageHandlers.Notifications
{
    public class NotificationsActionHelper : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenant, ActionMessage actionMessage, IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            var message = (NotificationMessage) actionMessage;
            var result = await SendNotificationEmail(tenant, message, (INotificationRepository) actionHandlerServiceRepository);
            Logger.Log($"Finished processing message with result: {result}", message, tenant, LogLevel.Info);
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
                Logger.Log($"No email settings provided for tenant {tenant.Id}", message, tenant, LogLevel.Error);
                return await Task.FromResult(SendEmailResult.Error);
            }

            //don't pass null values to the template
            var notificationEmail = new NotificationEmail(
                message.ProjectId,
                message.ProjectName ?? string.Empty,
                message.ArtifactId,
                message.ArtifactName ?? string.Empty,
                message.ArtifactUrl ?? string.Empty,
                message.MessageTemplate ?? string.Empty);

            var emailMessage = new Message
            {
                Subject = message.Subject,
                FromDisplayName = message.From,
                To = message.To.ToArray(),
                From = emailSettings.SenderEmailAddress,
                IsBodyHtml = true,
                Body = new NotificationEmailContent(notificationEmail).TransformText()
            };

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

            Logger.Log("Sending Email", message, tenant, LogLevel.Info);
            InternalSendEmail(smtpClientConfiguration, emailMessage, repository);
            Logger.Log("Email Sent", message, tenant, LogLevel.Info);
            return await Task.FromResult(SendEmailResult.Success);
        }

        //for unit testing
        private void InternalSendEmail(SMTPClientConfiguration smtpClientConfiguration, Message emailMessage, INotificationRepository repository)
        {
            repository.SendEmail(smtpClientConfiguration, emailMessage);
        }
    }
}
