using ArtifactStore.Models.Review;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtifactStore.Models
{
    public class ProjectApprovalStatus
    {
        public string StatusText { get; set; }

        public ApprovalType ApprovalType { get; set; }
    }
}
