using ServiceLibrary.Helpers;
using System.Runtime.Serialization;

namespace ServiceLibrary.Models
{
    [DataContract(Name = "DEmailSettings", Namespace = "http://schemas.datacontract.org/2004/07/BluePrintSys.RC.Data.AccessAPI.Model.Serializable")]
    public class EmailSettings
    {
        [DataMember(Name = "Id")]
        public string Id { get; set; }

        [DataMember(Name = "HostName")]
        public string HostName { get; set; }

        [DataMember(Name = "SenderEmailAddress")]
        public string SenderEmailAddress { get; set; }

        [DataMember(Name = "Port")]
        public int Port { get; set; }

        [DataMember(Name = "EnableSSL")]
        public bool EnableSSL { get; set; }

        [DataMember(Name = "Authenticated")]
        public bool Authenticated { get; set; }

        [DataMember(Name = "UserName")]
        public string UserName { get; set; }
        [DataMember(Name = "Password")]
        public string Password { get; set; }

        [DataMember(Name = "EnableNotifications")]
        public bool EnableNotifications { get; set; }

        [DataMember(Name = "EnableEmailDiscussion")]
        public bool EnableEmailDiscussion { get; set; }

        [DataMember(Name = "EnableEmailReplies")]
        public bool EnableEmailReplies { get; set; }

        [DataMember(Name = "Incoming_ServerType")]
        public int IncomingServerType { get; set; }

        [DataMember(Name = "Incoming_EnableSSL")]
        public bool IncomingEnableSSL { get; set; }

        [DataMember(Name = "Incoming_HostName")]
        public string IncomingHostName { get; set; }

        [DataMember(Name = "Incoming_Port")]
        public int IncomingPort { get; set; }

        [DataMember(Name = "Incoming_UserName")]
        public string IncomingUserName { get; set; }

        [DataMember(Name = "Incoming_Password")]
        public string IncomingPassword { get; set; }

        [DataMember(Name = "EnableAllUsers")]
        public bool EnableAllUsers { get; set; }

        [DataMember(Name = "EnableDomains")]
        public bool EnableDomains { get; set; }

        [DataMember(Name = "Domains")]
        public string Domains { get; set; }

        public static EmailSettings CreateFromString(string data)
        {
            return SerializationHelper.Deserialize<EmailSettings>(data);
        }
    }
}