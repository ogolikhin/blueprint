using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Models;

namespace AdminStore.Models.Emails
{
    public class TestEmailConfigInstanceSettings : IEmailConfigInstanceSettings
    {
        public string Id { get; set; }

        public string HostName { get; set; }

        public string SenderEmailAddress { get; set; }

        public int Port { get; set; }

        public bool EnableSSL { get; set; }

        public bool Authenticated { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public TestEmailConfigInstanceSettings(EmailOutgoingSettings outgoingSettings, string senderEmailAddress)
        {
            HostName = outgoingSettings.ServerAddress;
            SenderEmailAddress = senderEmailAddress;
            Port = outgoingSettings.Port;
            EnableSSL = outgoingSettings.EnableSsl;
            Authenticated = outgoingSettings.AuthenticatedSmtp;
            UserName = outgoingSettings.AuthenticatedSmtpUsername;
            Password = outgoingSettings.AuthenticatedSmtpPassword;
        }
    }
}
