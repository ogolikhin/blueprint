using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace AdminStore.Models
{
    [JsonObject]
    public class UpdateUserDto : CreationUserDto
    {
        /// <summary>
        /// The version of user.
        /// </summary>
        public int CurrentVersion { get; set; }
    }
}