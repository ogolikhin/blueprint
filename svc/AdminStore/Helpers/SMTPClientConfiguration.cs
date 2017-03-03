using System;

namespace AdminStore.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class SmtpClientConfiguration : BaseEmailServerConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        public SmtpClientConfiguration()
        {
            HostName = string.Empty;
            Port = 25;
            EnableSSL = false;
            Authenticated = false;
            UserName = string.Empty;
            Password = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Authenticated { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SenderEmailAddress { get; set; }
    }
}