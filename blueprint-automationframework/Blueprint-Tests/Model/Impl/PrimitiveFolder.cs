using Newtonsoft.Json;

namespace Model.Impl
{
    public class PrimitiveFolder : IPrimitiveFolder
    {
        [JsonProperty("Id")]
        public string FolderId { get; set; }

        [JsonProperty("ParentFolderId")]
        public string ParentFolderId { get; set; }

        [JsonProperty("Name")]
        public string FolderName { get; set; }

        [JsonProperty("Type")]
        public string InstanceType { get; set; }
    }
}
