﻿using System;
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

        /// <summary>
        /// Creates new AttachmentValue from INovaFile
        /// Use to pass into UpdateArtifact to add attachment
        /// </summary>
        public AttachmentValue(IUser user, INovaFile file)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(file, nameof(file));

            UserId = user.Id;
            UserName = user.Username;
            FileName = file.FileName; //500 error when it is empty?
            FileType = file.FileType;
            AttachmentId = null; //null for add, real id to telete existing attachment
            Guid = file.Guid;
            UploadedDate = null;
            ChangeType = 0; //0 for add, 2 for delete existing attachment 
        }

        /// <summary>
        /// Creates new AttachmentValue from AttachmentId
        /// Use to pass into UpdateArtifact to delete existing attachment
        /// </summary>
        public AttachmentValue(int attachmentId)
        {
            AttachmentId = attachmentId; //null for add, real id to telete existing attachment
            ChangeType = 2; //0 for add, 2 for delete existing attachment 
        }
    }
}
