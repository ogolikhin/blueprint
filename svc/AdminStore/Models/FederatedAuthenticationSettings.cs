using AdminStore.Helpers;
using System.Security.Cryptography.X509Certificates;

namespace AdminStore.Models
{
    public interface IFederatedAuthenticationSettings
    {
        string LoginUrl { get; }
        string LogoutUrl { get; }
        string ErrorUrl { get; }
        string NameClaimType { get; }

        X509Certificate2 Certificate { get; }
    }

    public class FederatedAuthenticationSettings : IFederatedAuthenticationSettings
    {
        public FederatedAuthenticationSettings(string settings, byte[] certificate)
        {
            Certificate = new X509Certificate2(certificate);
            var fedAuthSettings = SerializationHelper.Deserialize<SerializationHelper.FASettings>(settings);
            LoginUrl = fedAuthSettings.LoginUrl;
            LogoutUrl = fedAuthSettings.LogoutUrl;
            ErrorUrl = fedAuthSettings.ErrorUrl;
            NameClaimType = fedAuthSettings.NameClaimType;
        }

        public string LoginUrl { get; private set; }

        public string LogoutUrl { get; private set; }

        public string ErrorUrl { get; private set; }

        public string NameClaimType { get; private set; }

        public X509Certificate2 Certificate { get; private set; }
    }
}