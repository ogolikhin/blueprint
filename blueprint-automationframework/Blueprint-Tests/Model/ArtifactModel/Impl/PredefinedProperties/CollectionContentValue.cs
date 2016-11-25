using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ArtifactModel.Impl.PredefinedProperties
{
    public class CollectionContentValue
    {
        public List<int> AddedArtifacts { get; } = new List<int>();
        public List<int> RemovedArtifacts { get; } = new List<int>();
    }
}
