using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ArtifactModel.Impl
{
    public class CollectionItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ItemTypeId { get; set; }
        public int ItemTypePredefined { get; set; }
        public string Prefix { get; set; }
        public List<string> ArtifactPath { get; } = new List<string>();
    }
}
