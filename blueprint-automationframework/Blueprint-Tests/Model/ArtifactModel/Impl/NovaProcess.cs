using System.Collections.Generic;
using Model.StorytellerModel.Impl;

namespace Model.ArtifactModel.Impl
{
    // Mostly taken from: blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/Nova/Models/NovaProcess.cs
    public class NovaProcess : NovaArtifact
    {
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
        public List<KeyValuePair<int, int>> TempIdMap
        {
            get; set;
        }
    }
}
