﻿namespace AdminStore.Models
{
    public class LdapSettings
    {
        public int Id { get; set; }

        public string BindUser { get; set; }

        public string BindPassword { get; set; }

        public string LdapAuthenticationUrl { get; set; }

        public int AuthenticationType { get; set; }

        public string SettingName { get; set; }

        public bool EnableCustomSettings { get; set; }

        public string DomainAttribute { get; set; }

        public string AccountNameAttribute { get; set; }
    }
}