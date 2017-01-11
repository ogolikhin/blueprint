using Newtonsoft.Json;

namespace Model.Impl
{
    public interface IPrimitiveFolder
    {
        [JsonProperty("Id")]
        string FolderId { get; }

        [JsonProperty("Name")]
        string FolderName { get; }

        [JsonProperty("Type")]
        string InstanceType { get; }
    }
}
