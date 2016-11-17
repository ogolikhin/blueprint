using Newtonsoft.Json;

namespace Model.ArtifactModel.Impl
{
    // Copied from:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/Nova/Models/CopyNovaArtifactResultSet.cs
    [JsonObject]
    public class CopyNovaArtifactResultSet
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public NovaArtifactDetails Artifact { get; set; }

        public int CopiedArtifactsCount { get; set; }
    }
}
