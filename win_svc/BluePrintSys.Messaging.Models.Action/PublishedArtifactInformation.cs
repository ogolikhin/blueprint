using System.Collections.Generic;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class PublishedArtifactInformation
    {
        public int ProjectId { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public int Predefined { get; set; }

        public string Url { get; set; }

        public string BaseUrl { get; set; }

        public bool IsFirstTimePublished { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<PublishedPropertyInformation> ModifiedProperties { get; set; }

        public IEnumerable<int> AncestorArtifactTypeIds { get; set; }
    }
}
