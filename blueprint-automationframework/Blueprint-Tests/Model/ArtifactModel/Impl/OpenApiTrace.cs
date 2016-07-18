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

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="project">The project where the artifact exists.</param>
        /// <param name="artifact">The artifact being traced.</param>
        /// <param name="direction">The direction of the trace (To, From, Both).</param>
        /// <param name="traceType">The type of trace (ex. Manual).</param>
        /// <param name="isSuspect">Whether the trace is marked suspect.</param>
        public OpenApiTrace(IProject project, IArtifactBase artifact, TraceDirection direction, TraceTypes traceType, bool isSuspect)
            : this(project?.Id ?? -1, artifact, direction, traceType, isSuspect)
        {
            // Intentionally left blank.
        }

        /// <summary>
        /// Constructor taking a raw Project ID.
        /// </summary>
        /// <param name="projectId">The Project ID where the artifact exists.</param>
        /// <param name="artifact">The artifact being traced.</param>
        /// <param name="direction">The direction of the trace (To, From, Both).</param>
        /// <param name="traceType">The type of trace (ex. Manual).</param>
        /// <param name="isSuspect">Whether the trace is marked suspect.</param>
        public OpenApiTrace(int projectId, IArtifactBase artifact, TraceDirection direction, TraceTypes traceType, bool isSuspect)
            : this(projectId, artifact?.Id ?? -1, direction, traceType, isSuspect)
        {
            // Intentionally left blank.
        }

        /// <summary>
        /// This constructor is needed by Newtonsoft to deserialize the JSON.
        /// </summary>
        /// <param name="projectId">The Project ID where the artifact exists.</param>
        /// <param name="artifactId">The ID of the Artifact being traced.</param>
        /// <param name="direction">The direction of the trace (To, From, Both).</param>
        /// <param name="traceType">The type of trace (ex. Manual).</param>
        /// <param name="isSuspect">Whether the trace is marked suspect.</param>
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

        /// <summary>
        /// Compares this OpenApiTrace to another ITrace.
        /// </summary>
        /// <param name="trace">The other trace object to compare against.</param>
        /// <returns>True if the ITrace object properties are identical, otherwise false.</returns>
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
