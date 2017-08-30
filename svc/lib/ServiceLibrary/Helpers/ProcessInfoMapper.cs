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

            int processType;
            int processTypeValue = 0;
            if (Int32.TryParse(pi.ProcessType, out processType))
                processTypeValue = processType;

            switch (processTypeValue)
            {
                case (int)ProcessType.UserToSystemProcess:
                    result.ProcessType = ProcessType.UserToSystemProcess;
                    break;
                case (int)ProcessType.BusinessProcess:
                    result.ProcessType = ProcessType.BusinessProcess;
                    break;
                case (int)ProcessType.SystemToSystemProcess:
                    result.ProcessType = ProcessType.SystemToSystemProcess;
                    break;
                default:
                    result.ProcessType = ProcessType.None;
                    break;
            }

            return result;
        }

        public static List<ProcessInfoDto> Map(IEnumerable<ProcessInfo> pis)
        {
            return pis.Select(Map).ToList();
        }
    }
}
