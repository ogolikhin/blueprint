using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;

namespace Model.ArtifactModel.Impl
{
    public class OpenApiTrace : ITrace
    {
        #region Inherited from ITrace

        public int ProjectId { get; set; }

        public int ArtifactId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TraceDirection Direction { get; set; }

        [JsonProperty("Type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TraceTypes TraceType { get; set; }

        public bool IsSuspect { get; set; }

        #endregion Inherited from ITrace

        #region Additional Properties

        public string Message { get; set; }
        public int? ResultCode { get; set; }

        #endregion Additional Properties

        public OpenApiTrace(IProject project, IArtifactBase artifact, TraceDirection direction, TraceTypes traceType, bool isSuspect)
            : this(project?.Id ?? -1, artifact, direction, traceType, isSuspect)
        {
            // Intentionally left blank.
        }

        public OpenApiTrace(int projectId, IArtifactBase artifact, TraceDirection direction, TraceTypes traceType, bool isSuspect)
            : this(projectId, artifact?.Id ?? -1, direction, traceType, isSuspect)
        {
            // Intentionally left blank.
        }

        // This constructor is needed by Newtonsoft to deserialize the JSON.
        [JsonConstructor]
        public OpenApiTrace(int projectId, int artifactId, TraceDirection direction, TraceTypes traceType, bool isSuspect)
        {
            Assert.That(projectId >= 0, "The Project ID was {0}, but it cannot be negative!", projectId);
            Assert.That(projectId >= 0, "The Artifact ID was {0}, but it cannot be negative!", artifactId);

            ProjectId = projectId;
            ArtifactId = artifactId;
            Direction = direction;
            TraceType = traceType;
            IsSuspect = isSuspect;
        }

        public bool Equals(ITrace trace)
        {
            if (trace == null)
            {
                return false;
            }

            return ((ProjectId == trace.ProjectId) && (ArtifactId == trace.ArtifactId) 
                && (Equals(Direction, trace.Direction)) &&
                (Equals(TraceType, trace.TraceType)) && (IsSuspect == trace.IsSuspect));
        }
    }
}
