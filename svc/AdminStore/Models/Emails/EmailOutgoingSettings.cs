namespace AdminStore.Models.Emails
{
    public class EmailOutgoingSettings : BaseEmailSettings
    {
        public bool AuthenticatedSmtp { get; set; }
        public string AccountEmailAddress { get; set; }
    }
}
