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

        public TestEmailConfigInstanceSettings(EmailOutgoingSettings outgoingSettings)
        {
            HostName = outgoingSettings.ServerAddress;
            SenderEmailAddress = outgoingSettings.AccountEmailAddress;
            Port = outgoingSettings.Port;
            EnableSSL = outgoingSettings.EnableSsl;
            Authenticated = outgoingSettings.AuthenticatedSmtp;
            UserName = outgoingSettings.AccountUsername;
            Password = outgoingSettings.AccountPassword;
        }
    }
}
