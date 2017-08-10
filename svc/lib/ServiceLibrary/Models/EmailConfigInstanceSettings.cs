using System.Runtime.Serialization;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Models
{
    public interface IEmailConfigInstanceSettings
    {
        string Id { get; }

        string HostName { get; }

        string SenderEmailAddress { get; }

        int Port { get; }

        bool EnableSSL { get; }

        bool Authenticated { get; }

        string UserName { get; }

        string Password { get; }
    }

    public class EmailConfigInstanceSettings : IEmailConfigInstanceSettings
    {
        public EmailConfigInstanceSettings(string settings)
        {
            var emailSettings = SerializationHelper.Deserialize<ESSettings>(settings);
            Id = emailSettings.Id;
            HostName = emailSettings.HostName;
            SenderEmailAddress = emailSettings.SenderEmailAddress;
            Port = emailSettings.Port;
            EnableSSL = emailSettings.EnableSSL;
            Authenticated = emailSettings.Authenticated;
            UserName = emailSettings.UserName;
            Password = emailSettings.Password;
        }
        public string Id { get; private set; }
        public string HostName { get; private set; }
        public string SenderEmailAddress { get; private set; }
        public int Port { get; private set; }
        public bool EnableSSL { get; private set; }
        public bool Authenticated { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }

        [DataContract(Name = "DEmailSettings", Namespace = "http://schemas.datacontract.org/2004/07/BluePrintSys.RC.Data.AccessAPI.Model.Serializable")]
        internal class ESSettings
        {
            public ESSettings()
            {
                //When empty this value is initialized to default 'Username' in FederatedAuthenticationSettings
                //NameClaimType = "Username";
            }

            [DataMember]
            public string Id { get; set; }

            [DataMember]
            public string HostName { get; set; }

            [DataMember]
            public string SenderEmailAddress { get; set; }

            [DataMember]
            public int Port { get; set; }

            [DataMember]
            public bool EnableSSL { get; set; }

            [DataMember]
            public bool Authenticated { get; set; }

            [DataMember]
            public string UserName { get; set; }

            [DataMember]
            public string Password { get; set; }
        }
    }
}