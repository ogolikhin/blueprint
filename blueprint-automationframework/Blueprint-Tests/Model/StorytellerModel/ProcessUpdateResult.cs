using System.Collections.Generic;
using Model.StorytellerModel.Impl;
using Newtonsoft.Json;

namespace Model.StorytellerModel
{
    /// <summary>
    /// The Result Returned from UpdateProcess()
    /// 
    /// From: UPDATE svc/components/storyteller/processes/{0} 
    /// </summary>
    public class ProcessUpdateResult
    {
        [JsonProperty("messages")]
        public IEnumerable<OperationMessageResult> Messages{ get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [JsonProperty("tempIdMap")]
        public List<KeyValuePair<int, int>> TempIdMap { get; set; }
    }
}