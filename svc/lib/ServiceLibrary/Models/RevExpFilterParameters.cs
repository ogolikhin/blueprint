using System.Collections.Generic;

namespace ServiceLibrary.Models
{

    public class RevExpFilterParameters
    {
        public IEnumerable<int> ApprStsIds { get; set; }

        public bool? IsApprovalRequired { get; set; }
    }
}
