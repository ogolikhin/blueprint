namespace AdminStore.Models.Emails
{
    public class EmailOutgoingSettings : BaseEmailSettings
    {
        public bool AuthenticatedSmtp { get; set; }
        public bool IsPasswordDirty { get; set; }
        public string AccountEmailAddress { get; set; }
    }
}
