using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ArtifactModel.Impl.PredefinedProperties
{
    public class ActorInheritanceValue
    {
        public List<string> PathToProject { get; } = new List<string>();

        public string ActorName { get; set; }

        public string ActorPrefix { get; set; }

        public int ActorId { get; set; }

        public bool HasAccess { get; set; }
    }
}
