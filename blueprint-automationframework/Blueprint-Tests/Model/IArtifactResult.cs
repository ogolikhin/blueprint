using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public interface IArtifactResult
    {
        IArtifact Artifact { get; set; }
        string Message { get; set; }
        string ResultCode { get; set; }
    }
}
