namespace BluePrintSys.Messaging.Models.Actions
{
    public class EmailSettings
    {
        public string Id { get; set; }

        public string HostName { get; set; }

        public string SenderEmailAddress { get; set; }

        public int Port { get; set; }

        public bool EnableSsl { get; set; }

        public bool Authenticated { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool EnableNotifications { get; set; }

        public bool EnableEmailDiscussion { get; set; }

        public bool EnableEmailReplies { get; set; }

        public int IncomingServerType { get; set; }

        public bool IncomingEnableSsl { get; set; }


        public string IncomingHostName { get; set; }

        public int IncomingPort { get; set; }

        public string IncomingUserName { get; set; }

        public string IncomingPassword { get; set; }

        public bool EnableAllUsers { get; set; }

        public bool EnableDomains { get; set; }

        public string Domains { get; set; }
    }
}
