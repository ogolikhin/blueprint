using AdminStore.Services.Email;

namespace AdminStore.Models.Emails
{
    public class EmailIncomingSettings : BaseEmailSettings
    {
        public EmailClientType ServerType { get; set; }
    }
}
