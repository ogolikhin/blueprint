using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public interface IArtifactInfo
    {
        int Id { get; set; }
        int ProjectId { get; set; }
        string Name { get; set; }
        string TypePrefix { get; set; }
        int BaseItemTypePredefined { get; set; }
        string Link { get; set; }
    }
}
