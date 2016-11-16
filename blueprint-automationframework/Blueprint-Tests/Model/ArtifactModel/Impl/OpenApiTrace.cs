using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    // Found in:  blueprint-current/Source/BluePrintSys.RC.Api.Business/Models/TraceTypes.cs
    [Flags]
    public enum OpenApiTraceTypes
    {
        None = 0x0,
        Parent = 0x1,
        Child = 0x2,
        Manual = 0x4,
        Other = 0x8,
        Reuse = 0x10,
        All = 31    //None | Parent | Child | Manual | Other | Reuse
    }

    public class OpenApiTrace : ITrace
    {
        #region Inherited Serialized JSON Properties from ITrace

        [JsonProperty("Type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public OpenApiTraceTypes TraceType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TraceDirection Direction { get; set; }

        public int ProjectId { get; set; }

        public int ArtifactId { get; set; }

        public string ArtifactPropertyName { get; set; }

        public string Label { get; set; }

        public Uri BlueprintUrl { get; set; }

        public Uri Link { get; set; }

        public bool IsSuspect { get; set; }

        public int? SubArtifactId { get; set; } // Returned by the Add-Trace call.
        public string Message { get; set; }     // Returned by the Add-Trace call.
        public int? ResultCode { get; set; }    // Returned by the Add-Trace call.

        #endregion Inherited Serialized JSON Properties from ITrace

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="project">The project where the artifact exists.  Cannot be null.</param>
        /// <param name="artifact">The artifact being traced.  Cannot be null.</param>
        /// <param name="direction">The direction of the trace (To, From, Both).</param>
        /// <param name="traceType">The type of trace (ex. Manual).</param>
        /// <param name="isSuspect">Whether the trace is marked suspect.</param>
        /// <param name="subArtifactId">(optional) The ID of a sub-artifact of the target artifact being traced.</param>
        /// <exception cref="ArgumentNullException">If project or artifact are null.</exception>
        public OpenApiTrace(IProject project,
            IArtifactBase artifact,
            TraceDirection direction,
            OpenApiTraceTypes traceType,
            bool isSuspect,
            int? subArtifactId)
            : this(project?.Id, artifact, direction, traceType, isSuspect, subArtifactId)
        {
            // Intentionally left blank because the work is done in the chained constructor.
        }

        /// <summary>
        /// Constructor taking a raw Project ID.
        /// </summary>
        /// <param name="projectId">The Project ID where the artifact exists.  Cannot be null.</param>
        /// <param name="artifact">The artifact being traced.  Cannot be null.</param>
        /// <param name="direction">The direction of the trace (To, From, Both).</param>
        /// <param name="traceType">The type of trace (ex. Manual).</param>
        /// <param name="isSuspect">Whether the trace is marked suspect.</param>
        /// <param name="subArtifactId">(optional) The ID of a sub-artifact of the target artifact being traced.</param>
        /// <exception cref="ArgumentNullException">If projectId or artifact are null.</exception>
        public OpenApiTrace(int? projectId,
            IArtifactBase artifact,
            TraceDirection direction,
            OpenApiTraceTypes traceType,
            bool isSuspect,
            int? subArtifactId)
            : this(projectId, artifact?.Id, direction, traceType, isSuspect, subArtifactId)
        {
            // Intentionally left blank because the work is done in the chained constructor.
        }

        /// <summary>
        /// This constructor is needed by Newtonsoft to deserialize the JSON.
        /// </summary>
        /// <param name="projectId">The Project ID where the artifact exists.  Cannot be null.</param>
        /// <param name="artifactId">The ID of the Artifact being traced.  Cannot be null.</param>
        /// <param name="direction">The direction of the trace (To, From, Both).</param>
        /// <param name="traceType">The type of trace (ex. Manual).</param>
        /// <param name="isSuspect">Whether the trace is marked suspect.</param>
        /// <param name="subArtifactId">(optional) The ID of a sub-artifact of the target artifact being traced.</param>
        /// <exception cref="ArgumentNullException">If projectId or artifactId are null.</exception>
        [JsonConstructor]
        public OpenApiTrace(int? projectId,
            int? artifactId,
            TraceDirection direction,
            OpenApiTraceTypes traceType,
            bool isSuspect,
            int? subArtifactId)
        {
            ThrowIf.ArgumentNull(projectId, nameof(projectId));
            ThrowIf.ArgumentNull(artifactId, nameof(artifactId));

            ProjectId = projectId.Value;
            ArtifactId = artifactId.Value;
            SubArtifactId = subArtifactId;
            Direction = direction;
            TraceType = traceType;
            IsSuspect = isSuspect;
        }
    }
}
