using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ArtifactModel.Impl.PredefinedProperties
{
    public class ActorInheritanceValue
    {
        /// <summary>
        /// Path to the Inherited From actor. PathToProject[0] - name of project which contains Inherited From actor.
        /// </summary>
        public List<string> PathToProject { get; } = new List<string>();

        public string ActorName { get; set; }

        public string ActorPrefix { get; set; }

        public int ActorId { get; set; }

        public bool HasAccess { get; set; }
    }
}
