﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Utilities;

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

        public int? SubArtifactId { get; set; }
        public string Message { get; set; }
        public int? ResultCode { get; set; }

        #endregion Additional Properties

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
            TraceTypes traceType,
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
            TraceTypes traceType,
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
            TraceTypes traceType,
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
