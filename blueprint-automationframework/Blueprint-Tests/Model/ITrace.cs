using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public interface ITrace
    {
        string Type { get; set; }
        string Direction { get; set; }
        int ProjectId { get; set; }
        int ArtifactId { get; set; }
        string ArtifactPropertyName { get; set; }
        string Label { get; set; }
        string BlueprintUrl { get; set; }
        string Link { get; set; }
        bool IsSuspect { get; set; }
    }
}
