using System.Collections.Generic;

namespace ServiceLibrary.Models.Email
{
    public class Message
    {
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> To { get; set; }
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

        public DDiscussionEmail DiscussionEmail { get; set; }
    }
}
