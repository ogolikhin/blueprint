using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;

namespace AdminStore.Models
{
    [JsonObject]
    public class UserIcon
    {
        /// <summary>
        /// The image binary data.
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// The users's ID.
        /// </summary>
        public int UserId { get; set; }

    }
}
