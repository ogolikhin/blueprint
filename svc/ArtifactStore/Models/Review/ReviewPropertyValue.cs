using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtifactStore.Models.Review
{
    public class ReviewPropertyValue
    {
        public int StartRevision { get; set; }
        public string StringValue { get; set; }
        public int VersionItemId { get; set; }
        public int VersionProjectId { get; set; }
    }
}
