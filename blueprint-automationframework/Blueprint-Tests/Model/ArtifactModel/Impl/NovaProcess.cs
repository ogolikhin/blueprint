using Model.StorytellerModel.Impl;
using System.Collections.Generic;

namespace Model.ArtifactModel.Impl
{
    // Mostly taken from: blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/Nova/Models/NovaProcess.cs
    public class NovaProcess : NovaArtifactDetails, INovaProcess
    {
        /// <summary>
        /// Constructor needed to deserialize it as generic type.
        /// </summary>
        public NovaProcess()
        {
        }

        public Process Process { get; set; }
    }

    public class NovaProcessUpdateResult
    {
        public IEnumerable<OperationMessageResult> Messages
        {
            get; set;
        }

        public NovaArtifact Result
        {
            get; set;
        }

        // Mapping between temporary ids sent and actual ids received
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public List<KeyValuePair<int, int>> TempIdMap
        {
            get; set;
        }
    }
}
