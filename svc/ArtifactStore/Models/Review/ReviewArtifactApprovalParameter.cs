using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtifactStore.Models.Review
{
    public class ReviewArtifactApprovalParameter
    {
        public int ArtifactId { get; set; }

        public string Approval { get; set; }

        public ApprovalType ApprovalFlag { get; set; }
    }
}
