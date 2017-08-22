using AdminStore.Services.Email;

namespace AdminStore.Models.Emails
{
    public class EmailIncomingSettings
    {
        public string ServerAddress { get; set; }
        public EmailClientType ServerType { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string AccountUsername { get; set; }
        public string AccountPassword { get; set; }
        public bool IsPasswordDirty { get; set; }
    }
}
