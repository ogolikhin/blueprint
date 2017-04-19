using Model.Common.Constants;

namespace Model.NovaModel.Components.ImpactAnalysisService
{
    // Taken from blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/ImpactAnalysis/Models/ImpactAnalysisTree.cs
    public class ImpactAnalysisTree
    {
        public ImpactAnalysisNode Root { get; set; }

        public int Levels { get; private set; }

        public ImpactAnalysisTree(int levels = BusinessLayerConstants.ImpactAnalysisDefaultLevels)
        {
            Levels = levels;
        }
    }

}
