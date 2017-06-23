using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Models.Review
{
    public class AssignReviewerRolesParameter
    {
        public int UserId { get; set; }

        public ReviewParticipantRole Role { get; set; }
    }
}