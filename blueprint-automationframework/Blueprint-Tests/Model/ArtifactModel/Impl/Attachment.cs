using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Model.NovaModel;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    /// <summary>
    /// JSON received from svc/artifactstore/artifacts/{artifactId}/attachment
    /// </summary>
    public class Attachments
    {
        public int ArtifactId { get; set; }

        public int? SubartifactId { get; set; }

        [JsonProperty("attachments")]
        public List<AttachedFile> AttachedFiles { get; } = new List<AttachedFile>();

        [JsonProperty("documentReferences")]
        public List<DocumentReference> DocumentReferences { get; } = new List<DocumentReference>();
    }

    /// <summary>
    /// JSON received from svc/artifactstore/artifacts/{artifactId}/attachment
    /// </summary>
    public class AttachedFile
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public string FileName { get; set; }

        public int AttachmentId { get; set; }

        public DateTime UploadedDate { get; set; }
    }

    /// <summary>
    /// JSON received from svc/artifactstore/artifacts/{artifactId}/attachment
    /// </summary>
    public class DocumentReference
    {
        public string ArtifactName { get; set; }

        public int ArtifactId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public DateTime ReferencedDate { get; set; }
    }


    /// <summary>
    /// Class in use in NovaArtifactDetails to add attachment to the artifact
    /// </summary>
    public class AttachmentValue
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public string FileName { get; set; }

        public string FileType { get; set; }

        public int? AttachmentId { get; set; }

        public string Guid { get; set; }

        public DateTime? UploadedDate { get; set; }

        public int ChangeType { get; set; }

        public AttachmentValue(IUser user, INovaFile file)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(file, nameof(file));

            UserId = user.Id;
            UserName = user.Username;
            FileName = file.FileName; //500 error when it is empty?
            FileType = file.FileType;
            AttachmentId = null; //now it is always null?
            Guid = file.Guid;
            UploadedDate = null;
            ChangeType = 0; //now it is always 0?
        }
    }
}
