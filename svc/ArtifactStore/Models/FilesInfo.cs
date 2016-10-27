using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Models
{
    public class LinkedArtifactInfo
    {
        public int ArtifactId;
        public string ArtifactName;
        public string ItemTypePrefix;
    }

    public class FilesInfo
    {
        private readonly IList<Attachment> _attachments;
        private readonly IList<DocumentReference> _documentReferences;

        public FilesInfo(IList<Attachment> attachments, IList<DocumentReference> documentReferences)
        {
            _attachments = attachments;
            _documentReferences = documentReferences;
        }

        public int ArtifactId { get; set; }

        public int? SubartifactId { get; set; }

        public IList<Attachment> Attachments
        {
            get
            {
                return _attachments;
            }
        }

        public IList<DocumentReference> DocumentReferences
        {
            get { return _documentReferences; }
        }
    }

    //public abstract class AbstractAttachment
    //{
    //    public int Id { get; set; }
    //    public string FileName { get; set; }
    //}

    [JsonObject]
    public class Attachment
    {
        [JsonProperty]
        public int UserId { get; set; }
        [JsonProperty]
        public string UserName { get; set; }
        [JsonProperty]
        public string FileName { get; set; }
        [JsonProperty]
        public int AttachmentId { get; set; }
        [JsonProperty]
        public DateTime? UploadedDate { get; set; }
        [JsonIgnore]
        public Guid FileGuid { get; set; }
    }

    [JsonObject]
    public class DocumentReference
    {
        [JsonProperty]
        public string ArtifactName { get; set; }
        [JsonProperty]
        public int ArtifactId { get; set; }
        [JsonProperty]
        public int UserId { get; set; }
        [JsonProperty]
        public string UserName { get; set; }
        [JsonProperty]
        public string ItemTypePrefix { get; set; }
        [JsonProperty]
        public DateTime ReferencedDate { get; set; }
    }
}