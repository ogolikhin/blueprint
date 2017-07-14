namespace BluePrintSys.Messaging.Models.Actions
{
    public class PublishedArtifactInformation
    {
        public int ProjectId { get; set; }

        public int ArtifactId { get; set; }

        public int ArtifactTypeId { get; set; }

        public int ArtifactTypePredefined { get; set; }
    }
}
