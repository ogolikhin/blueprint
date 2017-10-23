namespace ServiceLibrary.Models.Email
{
    public class SMTPClientConfiguration : BaseEmailServerConfiguration
    {
        /// <summary>
        ///
        /// </summary>
        public SMTPClientConfiguration()
        {
            HostName = string.Empty;
            Port = 25;
            EnableSsl = false;
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
