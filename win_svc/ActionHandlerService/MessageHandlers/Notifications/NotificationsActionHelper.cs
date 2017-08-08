using System;
using System.Linq;
using System.Threading.Tasks;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers.Security;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Email;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Notification.Templates;

namespace ActionHandlerService.MessageHandlers.Notifications
{
    public class NotificationsActionHelper : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenantInformation, ActionMessage actionMessage, IActionHandlerServiceRepository repository)
        {
            var result = await SendNotificationEmail(tenantInformation, (NotificationMessage) actionMessage);
            return await Task.FromResult(result == SendEmailResult.Success);
        }

        private async Task<SendEmailResult> SendNotificationEmail(TenantInformation tenantInformation, NotificationMessage details)
        {
            var result = SendEmailResult.Success;
            try
            {
                //It should be responsibility of notification handler to recieve information about email settings
                //Maybe this information can be cached for tenant ids???
                var emailSettings = await (new EmailSettingsRetriever(tenantInformation.ConnectionString)).GetEmailSettings();
                if (emailSettings == null)
                {
                    Log.Error($"No email settings provided for tenant {tenantInformation.Id} ");
                    return await Task.FromResult(SendEmailResult.Error);
                }

                var message = BuildMessage(emailSettings, details);
                message.To = details.To.ToArray();

                var smtpClientConfiguration = new SMTPClientConfiguration
                {
                    Authenticated = emailSettings.Authenticated,
                    EnableSsl = emailSettings.EnableSSL,
                    HostName = emailSettings.HostName,

                    // Bug 61312: Password must be decrypted before saving in configuration
                    Password = SystemEncryptions.Decrypt(emailSettings.Password),

                    Port = emailSettings.Port,
                    UserName = emailSettings.UserName
                };

                InternalSendEmail(smtpClientConfiguration, message);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Notification failed. Exception details {0}", e);
                throw;
            }
            return await Task.FromResult(result);
        }

        private Message BuildMessage(EmailSettings emailSettings, NotificationMessage details)
        {
            var message = new Message
            {
                Subject = details.Subject,
                IsBodyHtml = true,
                From = emailSettings.SenderEmailAddress,
                Body = GetEmailBody(
                    new NotificationEmail(
                        details.ProjectId,
                        details.ProjectName,
                        details.ArtifactId,
                        details.ArtifactName,
                        details.ArtifactUrl,
                        details.MessageTemplate)),
                FromDisplayName = details.From,
                To = details.To.ToArray()
            };
            return message;
        }

        private string GetEmailBody(INotificationEmail details)
        {
            var emailBody = new NotificationEmailContent(details).TransformText();
            return emailBody;
        }

        //for unit testing
        private void InternalSendEmail(SMTPClientConfiguration smtpClientConfiguration, Message message)
        {
            new SmtpClient(smtpClientConfiguration).SendEmail(message);
        }
    }
}
