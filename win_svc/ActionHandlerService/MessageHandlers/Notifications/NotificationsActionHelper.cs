using System;
using System.Linq;
using System.Threading.Tasks;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers.Security;
using ServiceLibrary.Models.Email;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Notification.Templates;

namespace ActionHandlerService.MessageHandlers.Notifications
{
    public class NotificationsActionHelper : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenantInformation, ActionMessage actionMessage, IActionHandlerServiceRepository repository)
        {
            var result = await SendNotificationEmail((NotificationMessage) actionMessage);
            return await Task.FromResult(result == SendEmailResult.Success);
        }

        private Task<SendEmailResult> SendNotificationEmail(NotificationMessage details)
        {
            var result = SendEmailResult.Success;
            try
            {
                var message = BuildMessage(details);
                message.To = details.To.ToArray();

                var smtpClientConfiguration = new SMTPClientConfiguration
                {
                    Authenticated = details.EmailSettings.Authenticated,
                    EnableSsl = details.EmailSettings.EnableSSL,
                    HostName = details.EmailSettings.HostName,

                    // Bug 61312: Password must be decrypted before saving in configuration
                    Password = SystemEncryptions.Decrypt(details.EmailSettings.Password),

                    Port = details.EmailSettings.Port,
                    UserName = details.EmailSettings.UserName
                };

                InternalSendEmail(smtpClientConfiguration, message);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("RapidReviewNotification failed. Exception details {0}", e);
                result = SendEmailResult.Error;
            }
            return Task.FromResult(result);
        }

        private Message BuildMessage(NotificationMessage details)
        {
            var message = new Message
            {
                Subject = details.Subject,
                IsBodyHtml = true,
                From = details.EmailSettings.SenderEmailAddress,
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
