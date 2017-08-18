namespace AdminStore.Models.Emails
{
    public class EmailOutgoingSettings
    {
        public string ServerAddress { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public bool AuthenticatedSmtp { get; set; }
        public string AuthenticatedSmtpUsername { get; set; }
        public string AuthenticatedSmtpPassword { get; set; }
        public bool IsPasswordDirty { get; set; }
    }
}
