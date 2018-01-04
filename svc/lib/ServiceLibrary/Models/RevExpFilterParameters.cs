using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Models
{

    public class RevExpFilterParameters
    {
        public IEnumerable<int> ApprStsIds { get; set; } = new List<int>();

        public bool? IsApprovalRequired { get; set; }
    }
}
