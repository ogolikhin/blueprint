using System.Collections.Generic;

namespace Model.NovaModel.Components.ImpactAnalysisService
{
    // Taken from /BluePrintSys.RC.Business.Internal/Components/ImpactAnalysis/Models/ImpactAnalysisNode.cs
    public class ImpactAnalysisNode
    {
        #region service Exposed Properties	

        public int Id { get; set; }

        public string Name { get; set; }

        public string Prefix { get; set; }

        public int TypeId { get; set; }

        public bool IsLoop { get; set; }

        public bool IsUnauthorized { get; set; }

        public int? ParentId { get; set; }

        public bool IncludedIn { get; set; }

        public bool IsSuspect { get; set; }

        private List<ImpactAnalysisNode> _nodes;
        public List<ImpactAnalysisNode> Nodes
        {
            get
            {
                return _nodes ?? (_nodes = new List<ImpactAnalysisNode>());
            }
        }

        #endregion
    }
}