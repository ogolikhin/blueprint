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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819: Properties should not return arrays", Justification = "For JSON serialization, the property sometimes needs to be an array")]
        public byte[] Content { get; set; }

        /// <summary>
        /// The users's ID.
        /// </summary>
        public int UserId { get; set; }

    }
}
