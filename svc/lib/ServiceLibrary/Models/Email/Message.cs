namespace ServiceLibrary.Models.Email
{
    public class Message
    {
        /// <summary>
        /// 
        /// </summary>
        public string[] To { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FromDisplayName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsBodyHtml { get; set; }
    }
}
