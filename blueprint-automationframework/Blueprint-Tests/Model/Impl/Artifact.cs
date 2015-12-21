using System;
using System.Collections.Generic;
using Utilities.Facades;

namespace Model.Impl
{
    public class Artifact : IArtifact
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public int Version { get; set; }
        public int ParentId { get; set; }
        public Uri BlueprintUrl { get; set; }
        public int ArtifactTypeId { get; set; }
        public string ArtifactTypeName { get; set; }
        public string BaseArtifactType { get; set; }
        public bool AreTracesReadOnly { get; set; }
        public bool AreAttachmentsReadOnly { get; set; }
        public bool AreDocumentReferencesReadOnly { get; set; }
        public List<IAProperty> Properties { get; private set; }
        public List<IComment> Comments { get; }
        public List<ITrace> Traces { get; }
        public List<IAttachment> Attachments { get; }
        public void SetProperties(List <IAProperty> aproperty)
        {
            if (this.Properties == null)
            {
                Properties = new List<IAProperty>();
            }
            Properties = aproperty;
        }
    }
}
