using Newtonsoft.Json;
using System.Collections.Generic;

namespace Model.Impl
{
    // Taken from blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/ImpactAnalysis/Models/ImpactAnalysisResult.cs
    public class ImpactAnalysisResult
    {
        public ImpactAnalysisTree Tree { get; set; }

        public IEnumerable<ItemTypesInformation> TypesInfo { get; set; }

//      [JsonIgnore]
//      internal bool LimitForNodeGenerationReached { get; set; }
    }
}
