using System.Collections.Generic;
using ServiceLibrary.Helpers;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace AdminStore.Models
{
    public interface IFederatedAuthenticationSettings
    {
        string LoginUrl { get; }
        string LogoutUrl { get; }
        string ErrorUrl { get; }
        string NameClaimType { get; }
        bool IsAllowingNoDomain { get; }

        List<AllowedDomain> DomainList { get; }

        X509Certificate2 Certificate { get; }
    }

    public class AllowedDomain
    {
        public string Name { get; set; }
        public int Index { get; set; }
    }

    public class FederatedAuthenticationSettings : IFederatedAuthenticationSettings
    {
        public FederatedAuthenticationSettings(string settings, byte[] certificate)
        {
            Certificate = new X509Certificate2(certificate);
            var fedAuthSettings = SerializationHelper.Deserialize<FASettings>(settings);
            LoginUrl = fedAuthSettings.LoginUrl;
            LogoutUrl = fedAuthSettings.LogoutUrl;
            ErrorUrl = fedAuthSettings.ErrorUrl;
            IsAllowingNoDomain = fedAuthSettings.IsAllowingNoDomain;
            DomainList = new List<AllowedDomain>();
            fedAuthSettings.DomainList?.ForEach(d => DomainList.Add(new AllowedDomain
            {
                Index = d.Index,
                Name = d.Name
            }));
            
            NameClaimType = string.IsNullOrEmpty(fedAuthSettings.NameClaimType) ? "Username" : fedAuthSettings.NameClaimType;
        }

        public string LoginUrl { get; private set; }

        public string LogoutUrl { get; private set; }

        public string ErrorUrl { get; private set; }

        public string NameClaimType { get; private set; }
        public bool IsAllowingNoDomain { get; private set; }
        public List<AllowedDomain> DomainList { get; private set; }

        public X509Certificate2 Certificate { get; private set; }

        [DataContract(Name = "FederationAuthenticationSettingsHelper.FAAllowedDomian", Namespace = "http://schemas.datacontract.org/2004/07/BluePrintSys.RC.Data.AccessAPI.Impl")]
        internal class FAAllowedDomian
        {
            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public int Index { get; set; }
        }

        [DataContract(Name = "FederationAuthenticationSettingsHelper.FASettings", Namespace = "http://schemas.datacontract.org/2004/07/BluePrintSys.RC.Data.AccessAPI.Impl")]
        internal class FASettings
        {
            public FASettings()
            {
                //When empty this value is initialized to default 'Username' in FederatedAuthenticationSettings
                //NameClaimType = "Username";
            }

            [DataMember]
            public string LoginUrl { get; set; }

            [DataMember]
            public string LogoutUrl { get; set; }

            [DataMember]
            public string ErrorUrl { get; set; }

            [DataMember]
            public string LoginPrompt { get; set; }

            [DataMember]
            public string ESigPrompt { get; set; }

            [DataMember]
            public string NameClaimType { get; set; }

            [DataMember]
            public bool IsAllowingNoDomain { get; set; }

            [DataMember]
            public List<FAAllowedDomian> DomainList { get; set; }
        }
    }
}