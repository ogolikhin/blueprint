using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Models
{
    public class AssignArtifactsResult
    {
        public static AssignArtifactsResult Empty => new AssignArtifactsResult { Total = 0, AddedCount = 0 };
        public int Total { get; set; }
        public int AddedCount { get; set; }
    }
}
