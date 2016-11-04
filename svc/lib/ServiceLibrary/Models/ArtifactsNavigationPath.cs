using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Models
{
    public class ArtifactsNavigationPath
    {
        public int Level { get; set; }
        public int ArtifactId { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public int? ItemTypeId { get; set; }
    }
}
