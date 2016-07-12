using System;
using Newtonsoft.Json;

namespace Model.ArtifactModel.Impl
{
    public class OpenApiTrace
    {
        // TODO Change Type and Direction to enums

        public int ProjectId { get; set; }

        public int ArtifactId { get; set; }

        public string Direction { get; set; }

        [JsonProperty("Type")]
        public string TraceType { get; set; }

        public bool IsSuspect { get; set; }

        public string Message { get; set; }

        public int? ResultCode { get; set; }

        public OpenApiTrace(int projectId, int artifactId, string direction, string traceType,
            bool isSuspect)
        {
            ProjectId = projectId;
            ArtifactId = artifactId;
            Direction = direction;
            TraceType = traceType;
            IsSuspect = isSuspect;
        }

        public bool Equals(Trace trace)
        {
            if (trace == null)
            {
                return false;
            }
            else
            {
                return ((ProjectId == trace.ProjectId) && (ArtifactId == trace.ArtifactId) 
                    && (Equals(Direction, trace.Direction.ToString())) &&
                    (Equals(TraceType, trace.TraceType.ToString())) && (IsSuspect == trace.Suspect));
            }
        }
    }
}
