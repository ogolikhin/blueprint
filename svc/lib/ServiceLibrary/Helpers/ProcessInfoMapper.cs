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
            
            ProcessInfoDto result = new ProcessInfoDto { ItemId = pi.ItemId};

            ProcessType enumVal;
            ProcessType defaultValue = ProcessType.None;
            if (Enum.TryParse(pi.ProcessType, out enumVal))
            {
                result.ProcessType = enumVal;
            }
            else
            {
                result.ProcessType = defaultValue;
            }
            
            return result;
        }

        public static List<ProcessInfoDto> Map(IEnumerable<ProcessInfo> pis)
        {
            return pis.Select(Map).ToList();
        }
    }
}
