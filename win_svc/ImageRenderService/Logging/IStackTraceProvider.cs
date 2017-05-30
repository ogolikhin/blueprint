using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageRenderService.Logging
{
    public interface IStackTraceProvider
    {
        string GetStackTrace(int skipFrames);
    }
}
