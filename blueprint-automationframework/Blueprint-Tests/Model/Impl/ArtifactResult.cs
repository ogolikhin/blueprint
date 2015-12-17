using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Impl
{
    public class ArtifactResult : IArtifactResult
    {
        public IArtifact Artifact { get; set; }
        public string Message { get; set; }
        public string ResultCode { get; set; }
    }
}
