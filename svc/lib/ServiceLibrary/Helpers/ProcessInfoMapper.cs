using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Helpers
{
    public static class ProcessInfoMapper
    {
        public static ProcessInfoDto Map(ProcessInfo pi)
        {
            ProcessInfoDto result = new ProcessInfoDto{ItemId = pi.ItemId};

            if (pi.ProcessType.Equals(ProcessType.UserToSystemProcess.ToString()))
            {
                result.ProcessType = ProcessType.UserToSystemProcess;
            }
            else if (pi.ProcessType.Equals(ProcessType.BusinessProcess.ToString()))
            {
                result.ProcessType = ProcessType.BusinessProcess;
            }
            else if (pi.ProcessType.Equals(ProcessType.SystemToSystemProcess.ToString()))
            {
                result.ProcessType = ProcessType.SystemToSystemProcess;
            }
            else
            {
                result.ProcessType = ProcessType.None;
            }

            return result;
        }

        public static List<ProcessInfoDto> Map(IEnumerable<ProcessInfo> pis)
        {
            return pis.Select(Map).ToList();
        }
    }
}
