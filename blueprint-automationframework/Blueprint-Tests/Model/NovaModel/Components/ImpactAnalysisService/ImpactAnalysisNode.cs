using Newtonsoft.Json;
using System.Collections.Generic;

namespace Model.NovaModel.Components.ImpactAnalysisService
{
    // Taken from /BluePrintSys.RC.Business.Internal/Components/ImpactAnalysis/Models/ImpactAnalysisNode.cs
    public class ImpactAnalysisNode
    {
        #region service Exposed Properties	

        public int Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends Name, even if it's null.
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends Prefix, even if it's null.
        public string Prefix { get; set; }

        public int TypeId { get; set; }

        public bool IsLoop { get; set; }

        public bool IsUnauthorized { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends ParentId, even if it's null.
        public int? ParentId { get; set; }

        public bool IncludedIn { get; set; }

        public bool IsSuspect { get; set; }

        public bool IsRoot { get; set; }

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