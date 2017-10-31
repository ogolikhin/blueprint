using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtifactStore.Models.Review
{
    public class ParticipantMeaningOfSignatureResult
    {
        public int ParticipantId { get; set; }
        public int GroupId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int RoleAssignmentId { get; set; }
        public int MeaningOfSignatureId { get; set; }
        public string MeaningOfSignatureValue { get; set; }
    }
}
