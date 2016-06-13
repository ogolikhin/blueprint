using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Model.Impl
{
    [DataContract(Name = "InstanceProject", Namespace = "Model")]
    public class InstanceProject : Project
    {
        /// <summary>
        /// Specifies if project is accessible
        /// </summary>
        [JsonProperty("IsAccessible")]
        public string IsAccessible { get; set; }

        /// <summary>
        /// Specifies if project has children artifacts
        /// </summary>
        [JsonProperty("HasChildren")]
        public string HasChildren { get; set; }

        /// <summary>
        /// Specifies parent folder id
        /// </summary>
        [JsonProperty("ParentFolderId")]
        public string ParentFolderId { get; set; }
    }
}
