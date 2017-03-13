using System;
using Newtonsoft.Json;

namespace AdminStore.Models
{
    [JsonObject]
    public class ResetPasswordContent
    {
        [JsonProperty]
        public string Password { get; set; }
        [JsonProperty]
        public Guid Token { get; set; }
    }
}