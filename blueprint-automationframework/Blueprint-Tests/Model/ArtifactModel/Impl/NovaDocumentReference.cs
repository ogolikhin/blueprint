using Model.ArtifactModel.Enums;

namespace Model.ArtifactModel.Impl
{
    // Taken from:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/Nova/Models/NovaDocumentReference.cs
    public class NovaDocumentReference
    {
        public string ArtifactName { get; set; }
        public int ArtifactId { get; set; }
        public int UserId { get; set; }
        public int UserName { get; set; }
        public int itemTypePrefix { get; set; }
        public int referencedDate { get; set; }
        public ChangeType? ChangeType { get; set; }
    }
}
