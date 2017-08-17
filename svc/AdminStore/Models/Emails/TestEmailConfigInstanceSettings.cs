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
        public string Id { get; }

        public string HostName { get; }

        public string SenderEmailAddress { get; }

        public int Port { get; }

        public bool EnableSSL { get; }

        public bool Authenticated { get; }

        public string UserName { get; }

        public string Password { get; }

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
