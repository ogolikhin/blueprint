using System.Security.Cryptography.X509Certificates;

namespace AdminStore.Models
{
    public interface IFederatedAuthenticationSettings
    {
        string LoginUrl { get; }
        string LogoutUrl { get; }
        string ErrorUrl { get; }
        string NameClaimType { get; set; }

        X509Certificate2 Certificate { get; }
    }

    public class FederatedAuthenticationSettings
    {
        public bool IsEnabled { get; set; }

        public byte[] Certificate { get; set; }

        public string Settings { get; set; }
    }
}