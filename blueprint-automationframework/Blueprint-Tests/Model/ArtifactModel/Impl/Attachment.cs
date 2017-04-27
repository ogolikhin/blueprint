using Model.ArtifactModel.Enums;
using Model.NovaModel;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    /// <summary>
    /// JSON received from svc/artifactstore/artifacts/{artifactId}/attachment
    /// </summary>
    public class Attachments
    {
        /// <summary>
        /// This class describes which Attachments properties should be compared.
        /// </summary>
        public class CompareOptions
        {
            /// <summary>Should the AttachedFile.AttachmentId properties be compared?</summary>
            public bool CompareAttachmentIds { get; set; } = true;
            /// <summary>Should the AttachedFile.UploadedDate properties be compared?</summary>
            public bool CompareUploadedDates { get; set; } = true;

            /// <summary>Should the DocumentReference.ReferencedDate properties be compared?</summary>
            public bool CompareReferencedDates { get; set; } = true;

            /// <summary>Should the UserId and UserName properties of AttachedFile and DocumentReference be compared?</summary>
            public bool CompareUsers { get; set; } = true;
        }

        #region Serialized JSON Properties

        public int ArtifactId { get; set; }

        public int? SubartifactId { get; set; }

        [JsonProperty("attachments")]
        public List<AttachedFile> AttachedFiles { get; } = new List<AttachedFile>();

        [JsonProperty("documentReferences")]
        public List<DocumentReference> DocumentReferences { get; } = new List<DocumentReference>();

        #endregion Serialized JSON Properties

        /// <summary>
        /// Asserts that the two Attachments objects are equal.
        /// </summary>
        /// <param name="expectedAttachments">The expected Attachments.</param>
        /// <param name="actualAttachments">The actual Attachments.</param>
        /// <param name="compareOptions">(optional) Specifies which AttachedFile and DocumentReference properties to compare.
        ///     By default, all properties are compared.</param>
        /// <exception cref="AssertionException">If any properties are different.</exception>
        public static void AssertAreEqual(Attachments expectedAttachments, Attachments actualAttachments,
            CompareOptions compareOptions = null)
        {
            ThrowIf.ArgumentNull(expectedAttachments, nameof(expectedAttachments));
            ThrowIf.ArgumentNull(actualAttachments, nameof(actualAttachments));

            compareOptions = compareOptions ?? new CompareOptions();

            foreach (var expectedAttachment in expectedAttachments.AttachedFiles)
            {
                var actualAttachment = actualAttachments.AttachedFiles.Find(a => a.AttachmentId.Equals(expectedAttachment.AttachmentId));

                if (compareOptions.CompareAttachmentIds)
                {
                    Assert.NotNull(actualAttachment, "Couldn't find actual attachment with AttachmentId: {0}",
                        expectedAttachment.AttachmentId);

                    AttachedFile.AssertAreEqual(expectedAttachment, actualAttachment, compareOptions);
                }
            }

            foreach (var expectedDocumentReference in expectedAttachments.DocumentReferences)
            {
                var actualDocumentReference = actualAttachments.DocumentReferences.Find(a => a.ArtifactId.Equals(expectedDocumentReference.ArtifactId));

                Assert.NotNull(actualDocumentReference, "Couldn't find actual DocumentReference with ArtifactId: {0}", expectedDocumentReference.ArtifactId);
                DocumentReference.AssertAreEqual(expectedDocumentReference, actualDocumentReference, compareOptions);
            }
        }
    }

    /// <summary>
    /// JSON received from svc/artifactstore/artifacts/{artifactId}/attachment
    /// </summary>
    public class AttachedFile
    {
        #region Serialized JSON Properties

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string FileName { get; set; }

        public int AttachmentId { get; set; }

        public DateTime? UploadedDate { get; set; }

        #endregion Serialized JSON Properties

        /// <summary>
        /// Asserts that the two AttachedFiles are equal.
        /// </summary>
        /// <param name="expectedFile">The expected AttachedFile.</param>
        /// <param name="actualFile">The actual AttachedFile.</param>
        /// <param name="compareOptions">(optional) Specifies which AttachedFile properties to compare.  By default, all properties are compared.</param>
        /// <exception cref="AssertionException">If any properties are different.</exception>
        public static void AssertAreEqual(AttachedFile expectedFile, AttachedFile actualFile,
            Attachments.CompareOptions compareOptions = null)
        {
            ThrowIf.ArgumentNull(expectedFile, nameof(expectedFile));
            ThrowIf.ArgumentNull(actualFile, nameof(actualFile));

            compareOptions = compareOptions ?? new Attachments.CompareOptions();

            if (compareOptions.CompareAttachmentIds)
            {
                Assert.AreEqual(expectedFile.AttachmentId, actualFile.AttachmentId, "The AttachmentId properties don't match!");
            }

            if (compareOptions.CompareUploadedDates)
            {
                Assert.AreEqual(expectedFile.UploadedDate, actualFile.UploadedDate, "The UploadedDate properties don't match!");
            }

            if (compareOptions.CompareUsers)
            {
                Assert.AreEqual(expectedFile.UserId, actualFile.UserId, "The UserId properties don't match!");
                Assert.AreEqual(expectedFile.UserName, actualFile.UserName, "The UserName properties don't match!");
            }

            Assert.AreEqual(expectedFile.FileName, actualFile.FileName, "The FileName properties don't match!");
        }
    }

    /// <summary>
    /// JSON received from svc/artifactstore/artifacts/{artifactId}/attachment
    /// </summary>
    public class DocumentReference
    {
        #region Serialized JSON Properties

        public string ArtifactName { get; set; }

        public int ArtifactId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public DateTime ReferencedDate { get; set; }

        #endregion Serialized JSON Properties

        /// <summary>
        /// Asserts that both DocumentReferences are equal.
        /// </summary>
        /// <param name="expectedDocument">The expected DocumentReference.</param>
        /// <param name="actualDocument">The actual DocumentReference.</param>
        /// <param name="compareOptions">(optional) Specifies which DocumentReference properties to compare.  By default, all properties are compared.</param>
        /// <exception cref="AssertionException">If any of the properties don't match.</exception>
        public static void AssertAreEqual(DocumentReference expectedDocument, DocumentReference actualDocument,
            Attachments.CompareOptions compareOptions = null)
        {
            ThrowIf.ArgumentNull(expectedDocument, nameof(expectedDocument));
            ThrowIf.ArgumentNull(actualDocument, nameof(actualDocument));

            compareOptions = compareOptions ?? new Attachments.CompareOptions();

            Assert.AreEqual(expectedDocument.ArtifactId, actualDocument.ArtifactId, "The ArtifactId properties don't match!");
            Assert.AreEqual(expectedDocument.ArtifactName, actualDocument.ArtifactName, "The ArtifactName properties don't match!");

            if (compareOptions.CompareReferencedDates)
            {
                Assert.AreEqual(expectedDocument.ReferencedDate, actualDocument.ReferencedDate, "The ReferencedDate properties don't match!");
            }

            if (compareOptions.CompareUsers)
            {
                Assert.AreEqual(expectedDocument.UserId, actualDocument.UserId, "The UserId properties don't match!");
                Assert.AreEqual(expectedDocument.UserName, actualDocument.UserName, "The UserName properties don't match!");
            }
        }
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
        /// Creates new AttachmentValue from INovaFile.
        /// Use to pass into UpdateArtifact to add attachment.
        /// </summary>
        /// <param name="user">The user that will add the attachment.</param>
        /// <param name="file">The file to be attached.</param>
        /// <param name="changeType">(optional) The type of change.  By default it adds the specified attachment.</param>
        public AttachmentValue(IUser user, INovaFile file, ChangeType changeType = ChangeType.Create)
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
            ChangeType = changeType;
        }

        /// <summary>
        /// Creates new AttachmentValue from AttachmentId.
        /// Use to pass into UpdateArtifact to delete existing attachment.
        /// </summary>
        /// <param name="attachmentId">The ID of the attachment.</param>
        /// <param name="changeType">(optional) The type of change.  By default it deletes the specified attachment.</param>
        public AttachmentValue(int attachmentId, ChangeType changeType = ChangeType.Delete)
        {
            AttachmentId = attachmentId; //null for add, real id to delete existing attachment
            ChangeType = changeType;
        }
    }
}
