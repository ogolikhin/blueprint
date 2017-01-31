using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Model.Impl
{
    // TODO: Should rename this as InstanceItem as defined in svc/AdminStore/Models/InstanceItem.cs
    // TODO: Should not inherits from Project
    [DataContract(Name = "InstanceProject", Namespace = "Model")]
    public class InstanceProject : Project
    {
        #region Properties

        /// <summary>
        /// Specifies if project is accessible
        /// </summary>
        [JsonIgnore]
        public bool? IsAccessible { get; set; }

        #region Serialized JSON Properties

        /// <summary>
        /// Specifies if project has children artifacts
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasChildren { get; set; }

        /// <summary>
        /// Specifies parent folder id
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ParentFolderId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        [JsonProperty("Type")]
        public InstanceItemTypeEnum Type { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public RolePermissions? Permissions { get; set; }

        #endregion Serialized JSON Properties

        #endregion Properties

    }

    public enum InstanceItemTypeEnum
    {
        Folder = 0,
        Project = 1
    }
}
