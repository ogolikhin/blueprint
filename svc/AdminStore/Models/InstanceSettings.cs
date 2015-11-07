namespace AdminStore.Models
{
    public class InstanceSettings
    {
        public bool UseDefaultConnection { get; set; }

        public bool EnableLDAPIntegration { get; set; }

        public int PasswordExpirationInDays { get; set; }
    }
}