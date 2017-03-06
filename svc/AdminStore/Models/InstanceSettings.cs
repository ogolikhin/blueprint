namespace AdminStore.Models
{
    public class InstanceSettings
    {
        public bool UseDefaultConnection { get; set; }

        public bool IsLdapIntegrationEnabled { get; set; }

        public int PasswordExpirationInDays { get; set; }

        public bool? IsSamlEnabled { get; set; }

        public int MaximumInvalidLogonAttempts { get; set; }

        public EmailConfigInstanceSettings EmailSettingsDeserialized {get; set; }

        public string EmailSettings { get; set; }
    }
}