using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Services.Email;
using ServiceLibrary.Models;

namespace AdminStore.Models.Emails
{
    public class EmailSettingsDto
    {
        public bool EnableReviewNotifications { get; set; }
        public bool EnableEmailNotifications { get; set; }
        public bool EnableDiscussions { get; set; }
        public EmailIncomingSettings Incoming { get; set; }
        public EmailOutgoingSettings Outgoing { get; set; }

        public static explicit operator EmailSettingsDto(EmailSettings settings)
        {
            return new EmailSettingsDto()
            {
                EnableDiscussions = settings.EnableEmailReplies,
                EnableEmailNotifications = settings.EnableEmailDiscussion,
                EnableReviewNotifications = settings.EnableNotifications,
                Incoming = new EmailIncomingSettings()
                {
                    AccountPassword = null,
                    AccountUsername = settings.IncomingUserName,
                    ServerAddress = settings.IncomingHostName,
                    Port = settings.IncomingPort,
                    IsPasswordDirty = false,
                    EnableSsl = settings.IncomingEnableSSL,
                    ServerType = (EmailClientType)settings.IncomingServerType,
                    HasPassword = !string.IsNullOrWhiteSpace(settings.IncomingPassword)
                },
                Outgoing = new EmailOutgoingSettings()
                {
                    AccountEmailAddress = settings.SenderEmailAddress,
                    AccountPassword = null,
                    AccountUsername = settings.UserName,
                    AuthenticatedSmtp = settings.Authenticated,
                    EnableSsl = settings.EnableSSL,
                    Port = settings.Port,
                    ServerAddress = settings.HostName,
                    IsPasswordDirty = false,
                    HasPassword = !string.IsNullOrWhiteSpace(settings.Password)
                }
            };
        }
    }
}
