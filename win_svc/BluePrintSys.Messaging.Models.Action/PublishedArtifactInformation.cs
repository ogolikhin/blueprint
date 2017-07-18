using System.Collections.Generic;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class PublishedArtifactInformation
    {
        public int ProjectId { get; set; }

        public int Id { get; set; }

        public int Predefined { get; set; }

        public string Url { get; set; }

        public bool IsFirstTimePublished { get; set; }

        public List<PublishedPropertyInformation> ModifiedProperties { get; set; }
    }
}
