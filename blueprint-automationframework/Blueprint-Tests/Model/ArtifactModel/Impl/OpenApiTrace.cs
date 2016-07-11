using System;

namespace Model.ArtifactModel.Impl
{
    public class OpenApiTrace
    {
        // TODO Change Type and Direction to enums

        public int ProjectId { get; set; }

        public int ArtifactId { get; set; }

        public string Direction { get; set; }

        public string Type { get; set; }

        public bool IsSuspect { get; set; }

        public string Message { get; set; }

        public int? ResultCode { get; set; }
    }
}
