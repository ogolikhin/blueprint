using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ArtifactModel.Impl
{
    /// <summary>
    /// Describes discussion body that needs to be sent in request for discussion create/update call
    /// </summary>
    public class JsonDiscussionCallBody
    {
        public string Comment { get; set; }
        public int Status { get; set; }
    }
}
