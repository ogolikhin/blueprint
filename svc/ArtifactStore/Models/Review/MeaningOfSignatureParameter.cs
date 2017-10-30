using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtifactStore.Models.Review
{
    public class MeaningOfSignatureParameter
    {
        public int ParticipantId { get; set; }
        public int MeaningOfSignatureId { get; set; }
        public bool Adding { get; set; }
    }
}
