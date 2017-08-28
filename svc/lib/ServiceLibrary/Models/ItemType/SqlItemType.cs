using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Models.ItemType
{
    public class SqlItemType
    {
        public int ItemTypeId { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public int? InstanceTypeId { get; set; }
        public int Predefined { get; set; }
        public string Prefix { get; set; }
        public bool UsedInThisProject { get; set; }
        public int? WorkflowId { get; set; }
        public int StartRevision { get; set; }
        public int EndRevision { get; set; }
    }
}
