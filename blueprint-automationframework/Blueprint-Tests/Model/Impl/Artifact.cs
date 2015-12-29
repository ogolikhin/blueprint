using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utilities;

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
        [JsonConverter(typeof(Deserialization.ConcreteConverter<Property>))]
        public List<IProperty> Properties { get; private set; }
        [JsonConverter(typeof(Deserialization.ConcreteConverter<Comment>))]
        public List<IComment> Comments { get; }
        [JsonConverter(typeof(Deserialization.ConcreteConverter<Trace>))]
        public List<ITrace> Traces { get; }
        [JsonConverter(typeof(Deserialization.ConcreteConverter<Attachment>))]
        public List<IAttachment> Attachments { get; }
        public void SetProperties(List <IProperty> aproperty)
        {
            if (this.Properties == null)
            {
                Properties = new List<IProperty>();
            }
            Properties = aproperty;
        }
    }
}
