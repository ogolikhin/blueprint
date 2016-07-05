using System;
using Newtonsoft.Json;
using System.Collections.Generic;
namespace Model.ArtifactModel.Impl
{
    public class Attachment
    {
        public int ArtifactId { get; set; }

        public int? SubartifactId { get; set; }

        [JsonProperty("attachments")]
        public List<AttachedFile> AttachedFiles { get; } = new List<AttachedFile>();

        [JsonProperty("documentReferences")]
        public List<DocumentReference> DocumentReferences { get; } = new List<DocumentReference>();
    }

    public class AttachedFile
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public string FileName { get; set; }

        public int AttachmentId { get; set; }

        public DateTime UploadedDate { get; set; }
    }

    public class DocumentReference
    {
        public string ArtifactName { get; set; }

        public int ArtifactId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public DateTime ReferencedDate { get; set; }
    }
}
