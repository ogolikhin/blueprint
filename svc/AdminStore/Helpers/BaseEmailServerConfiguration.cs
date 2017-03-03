using System;
using System.Runtime.Serialization;

namespace AdminStore.Helpers
{
    //[KnownType(typeof(IncomingServerConfiguration))]
    [KnownType(typeof(SmtpClientConfiguration))]
    [Serializable]
    public class BaseEmailServerConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool EnableSSL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BaseEmailServerConfiguration()
        {
            HostName = string.Empty;
            UserName = string.Empty;
            Password = string.Empty;
        }
    }
}