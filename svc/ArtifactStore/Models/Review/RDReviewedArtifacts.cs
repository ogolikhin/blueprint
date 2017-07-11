using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ArtifactStore.Models.Review
{
    [DataContract(Namespace = "http://www.blueprintsys.com/raptor/reviews")]
    internal class RDReviewedArtifacts
    {
        [DataMember]
        public List<ReviewArtifactXml> ReviewedArtifacts;
    }

    [DataContract(Namespace = "http://www.blueprintsys.com/raptor/reviews", Name = "RA")]
    internal class ReviewArtifactXml
    {
        [DataMember(Name = "A", EmitDefaultValue = false)]
        public string Approval;

        [DataMember(Name = "AF", EmitDefaultValue = false)]
        public ApprovalType ApprovalFlag;

        [DataMember(Name = "Id", EmitDefaultValue = false)]
        public int ArtifactId;

        [DataMember(Name = "V", EmitDefaultValue = false)]
        public int ArtifactVersion;

        [DataMember(Name = "VS", EmitDefaultValue = false)]
        public ViewStateType ViewState;

        [DataMember(Name = "EO", EmitDefaultValue = false)]
        public DateTime? ESignedOn;
    }
}
