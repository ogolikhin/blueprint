using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Model.ArtifactModel.Enums;
using Model.NovaModel;
using Utilities;
using NUnit.Framework;

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

        public DateTime? UploadedDate { get; set; }

        /// <summary>
        /// Asserts that the two AttachedFiles are equal.
        /// </summary>
        /// <param name="expectedFile">The expected AttachedFile.</param>
        /// <param name="actualFile">The actual AttachedFile.</param>
        /// <param name="skipAttachmentIds">(optional) Pass true if you want to skip comparison of the AttachmentId properties.</param>
        /// <param name="skipUploadedDates">(optional) Pass true if you want to skip comparison of the UploadedDate properties.</param>
        /// <exception cref="AssertionException">If any properties are different.</exception>
        public static void AssertEquals(AttachedFile expectedFile, AttachedFile actualFile, bool skipAttachmentIds = false, bool skipUploadedDates = false)
        {
            ThrowIf.ArgumentNull(expectedFile, nameof(expectedFile));
            ThrowIf.ArgumentNull(actualFile, nameof(actualFile));

            if (!skipAttachmentIds)
            {
                Assert.AreEqual(expectedFile.AttachmentId, actualFile.AttachmentId, "The AttachmentId properties don't match!");
            }

            if (!skipUploadedDates)
            {
                Assert.AreEqual(expectedFile.UploadedDate, actualFile.UploadedDate, "The UploadedDate properties don't match!");
            }

            Assert.AreEqual(expectedFile.FileName, actualFile.FileName, "The FileName properties don't match!");
            Assert.AreEqual(expectedFile.UserId, actualFile.UserId, "The UserId properties don't match!");
            Assert.AreEqual(expectedFile.UserName, actualFile.UserName, "The UserName properties don't match!");
        }
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

        public ChangeType ChangeType { get; set; }

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
            AttachmentId = null; //null for add, real id to delete existing attachment
            Guid = file.Guid;
            UploadedDate = null;
            ChangeType = ChangeType.Create;
        }

        /// <summary>
        /// Creates new AttachmentValue from AttachmentId
        /// Use to pass into UpdateArtifact to delete existing attachment
        /// </summary>
        public AttachmentValue(int attachmentId)
        {
            AttachmentId = attachmentId; //null for add, real id to delete existing attachment
            ChangeType = ChangeType.Delete;
        }
    }
}
