using System;

namespace Model.ArtifactModel.Impl
{
    public class OpenApiTrace : ITrace
    {
        public string Type { get; set; }
        public string Direction { get; set; }
        public int ProjectId { get; set; }
        public int ArtifactId { get; set; }
        public string ArtifactPropertyName { get; set; }
        public string Label { get; set; }
        public Uri BlueprintUrl { get; set; }
        public string Link { get; set; }
        public bool IsSuspect { get; set; }
    }
}
