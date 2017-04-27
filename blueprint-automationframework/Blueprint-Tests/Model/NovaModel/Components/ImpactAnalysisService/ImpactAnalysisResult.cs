using Model.NovaModel.Metadata;
using System.Collections.Generic;

namespace Model.NovaModel.Components.ImpactAnalysisService
{
    // Taken from blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/ImpactAnalysis/Models/ImpactAnalysisResult.cs and modified
    public class ImpactAnalysisResult
    {
        public ImpactAnalysisTree Tree { get; set; }

        public List<ItemTypeInformation> TypesInfo { get; set; }
    }
}
