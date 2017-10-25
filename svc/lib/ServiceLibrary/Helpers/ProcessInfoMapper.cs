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
        public static ProcessInfoDto Map(ProcessInfo processInfo)
        {
            var result = new ProcessInfoDto { ItemId = processInfo.ItemId };

            ProcessType enumVal;
            result.ProcessType = Enum.TryParse(processInfo.ProcessType, out enumVal) ? enumVal : ProcessType.None;

            return result;
        }

        public static List<ProcessInfoDto> Map(IEnumerable<ProcessInfo> processInfos)
        {
            return processInfos.Select(Map).ToList();
        }
    }
}
