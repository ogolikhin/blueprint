using System.Collections.Generic;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DProject
    {
        public int? Id { get; set; }
        public string Path { get; set; }
        public IEnumerable<DArtifactType> ArtifactTypes { get; set; }
    }
}