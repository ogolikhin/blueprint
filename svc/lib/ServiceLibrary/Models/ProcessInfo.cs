using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models
{
    public class ProcessInfo
    {
        public int ItemId { get; set; }
        public ProcessType ProcessType { get; set; }
    }
}
