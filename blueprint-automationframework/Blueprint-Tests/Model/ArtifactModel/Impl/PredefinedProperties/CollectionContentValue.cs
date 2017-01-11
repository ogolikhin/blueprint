using System.Collections.Generic;

namespace Model.ArtifactModel.Impl.PredefinedProperties
{
    public class CollectionContentValue
    {
        public List<int> AddedArtifacts { get; } = new List<int>();
        public List<int> RemovedArtifacts { get; } = new List<int>();
    }
}
