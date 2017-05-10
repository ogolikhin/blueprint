using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl.PredefinedProperties;
using Model.NovaModel;
using Model.Factories;
using Newtonsoft.Json;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Common;
using Model.ArtifactModel.Enums;
using Model.ModelHelpers;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class DocumentFileTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region Positive tests

        [TestCase]
        [TestRail(166132)]
        [Description("Create and publish document, attach file, check that artifact details has expected values.")]
        public void AddAttachment_PublishedDocument_ArtifactHasExpectedDetails()
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(2048, "2KB_File.txt", "text/plain");

            //Currently Nova set ExpireTime 2 days from today for newly uploaded file
            var expireTime = System.DateTime.Now.AddDays(2);

            var uploadedFile = Helper.FileStore.AddFile(file, author, expireTime: expireTime, useMultiPartMime: true);
            Assert.IsNotNull(Helper.FileStore.GetSQLExpiredTime(uploadedFile.Guid), "Uploaded file shouldn't have null ExpiredTime");

            var artifact = CreateAndPublishDocumentArtifact(author, _project);
            artifact.Lock(author);

            // Execute & Verify:
            UpdateDocumentFile_CanGetAttachment(author, artifact, uploadedFile);
        }

        [TestCase]
        [TestRail(166151)]
        [Description("Create and publish document, attach file, delete attachment, check that artifact has no attachments.")]
        public void DeleteAttachment_PublishedDocumentWithAttachment_ArtifactHasNoAttachment()
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(2048, "2KB_File.txt", "text/plain");

            //Currently Nova set ExpireTime 2 days from today for newly uploaded file
            var expireTime = System.DateTime.Now.AddDays(2);

            var uploadedFile = Helper.FileStore.AddFile(file, author, expireTime: expireTime, useMultiPartMime: true);
            Assert.IsNotNull(Helper.FileStore.GetSQLExpiredTime(uploadedFile.Guid), "Uploaded file shouldn't have null ExpiredTime");

            var artifact = CreateAndPublishDocumentArtifact(author, _project);
            artifact.Lock(author);

            UpdateDocumentFile_CanGetAttachment(author, artifact, uploadedFile);

            artifact.Publish(author);
            artifact.Lock(author);

            // Execute & Verify:
            DeleteDocumentFile_CheckAttachmentIsEmpty(author, artifact);
        }

        [TestCase]
        [TestRail(166154)]
        [Description("Create and publish document, attach file, publish, replace attachment, check that artifact details has expected values.")]
        public void ReplaceAttachment_PublishedDocumentWithAttachment_ArtifactHasExpectedDetails()
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var file1 = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(2048, "2KB_File.txt", "text/plain");
            var file2 = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(4096, "4KB_File.txt", "text/plain");

            //Currently Nova set ExpireTime 2 days from today for newly uploaded file
            var expireTime = System.DateTime.Now.AddDays(2);

            var uploadedFile1 = Helper.FileStore.AddFile(file1, author, expireTime: expireTime, useMultiPartMime: true);
            Assert.IsNotNull(Helper.FileStore.GetSQLExpiredTime(uploadedFile1.Guid), "Uploaded file shouldn't have null ExpiredTime");

            var artifact = CreateAndPublishDocumentArtifact(author, _project);
            artifact.Lock(author);

            UpdateDocumentFile_CanGetAttachment(author, artifact, uploadedFile1);
            artifact.Publish(author);
            artifact.Lock(author);

            var uploadedFile2 = Helper.FileStore.AddFile(file2, author, expireTime: expireTime, useMultiPartMime: true);

            // Execute & Verify:
            UpdateDocumentFile_CanGetAttachment(author, artifact, uploadedFile2);
        }

        #endregion Positive tests

        #region 403 Forbidden

        [TestCase]
        [TestRail(227302)]
        [Description("Create and publish document & attach file with user that does not have permissions.  Verify returns 403 Forbidden.")]
        public void AddAttachment_PublishedDocument_UserWithNoPermissions_403Forbidden()
        {
            // Setup:
            var file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(2048, "2KB_File.txt", "text/plain");

            //Currently Nova set ExpireTime 2 days from today for newly uploaded file
            var expireTime = System.DateTime.Now.AddDays(2);

            var uploadedFile = Helper.FileStore.AddFile(file, _user, expireTime: expireTime, useMultiPartMime: true);
            Assert.IsNotNull(Helper.FileStore.GetSQLExpiredTime(uploadedFile.Guid), "Uploaded file shouldn't have null ExpiredTime");

            var artifact = CreateAndPublishDocumentArtifact(_user, _project);
            CreateAndPopulateDocumentFileValue(artifact, file);

            var userWithNoPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _project, artifact);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => artifact.Update(userWithNoPermissions, artifact.Artifact),
                "Updating artifact should return 403 Forbidden if user doesn't have permissions to artifact!");

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("You do not have permission to access the artifact (ID: {0})", artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, expectedExceptionMessage);

            Assert.IsNotNull(Helper.FileStore.GetSQLExpiredTime(file.Guid), "ExpiredTime for file should not be null if artifact is not saved!");
        }

        [TestCase]
        [TestRail(227303)]
        [Description("Create and publish document, attach file & delete attachment with user that does not have permissions.  Verify returns 403 Forbidden.")]
        public void DeleteAttachment_PublishedDocumentWithAttachment_UserWithNoPermissions_403Forbidden()
        {
            // Setup:
            var file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(2048, "2KB_File.txt", "text/plain");

            //Currently Nova set ExpireTime 2 days from today for newly uploaded file
            var expireTime = System.DateTime.Now.AddDays(2);

            var uploadedFile = Helper.FileStore.AddFile(file, _user, expireTime: expireTime, useMultiPartMime: true);
            Assert.IsNotNull(Helper.FileStore.GetSQLExpiredTime(uploadedFile.Guid), "Uploaded file shouldn't have null ExpiredTime");

            var artifact = CreateAndPublishDocumentArtifact(_user, _project);
            artifact.Lock(_user);

            UpdateDocumentFile_CanGetAttachment(_user, artifact, uploadedFile);
            
            artifact.Publish(_user);

            var userWithNoPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _project, artifact);

            artifact.DocumentFile = null;

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => artifact.Update(userWithNoPermissions, artifact.Artifact),
                "Updating artifact should return 403 Forbidden if user doesn't have permissions to artifact!");

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("You do not have permission to access the artifact (ID: {0})", artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, expectedExceptionMessage);

            var updatedArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            Assert.IsNotNull(updatedArtifactDetails.SpecificPropertyValues[0].CustomPropertyValue, "File information should still be existing.  Value should not be null!");
        }

        [TestCase]
        [TestRail(227304)]
        [Description("Create and publish document, attach file, publish & replace attachment with user that does not have permissions.   Verify returns 403 Forbidden.")]
        public void ReplaceAttachment_PublishedDocumentWithAttachment_UserWithNoPermissions_403Forbidden()
        {
            // Setup:
            var file1 = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(2048, "2KB_File.txt", "text/plain");
            var file2 = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(4096, "4KB_File.txt", "text/plain");

            //Currently Nova set ExpireTime 2 days from today for newly uploaded file
            var expireTime = System.DateTime.Now.AddDays(2);

            var uploadedFile1 = Helper.FileStore.AddFile(file1, _user, expireTime: expireTime, useMultiPartMime: true);
            Assert.IsNotNull(Helper.FileStore.GetSQLExpiredTime(uploadedFile1.Guid), "Uploaded file shouldn't have null ExpiredTime");

            var artifact = CreateAndPublishDocumentArtifact(_user, _project);
            artifact.Lock(_user);

            UpdateDocumentFile_CanGetAttachment(_user, artifact, uploadedFile1);
            artifact.Publish(_user);

            var uploadedFile2 = Helper.FileStore.AddFile(file2, _user, expireTime: expireTime, useMultiPartMime: true);

            CreateAndPopulateDocumentFileValue(artifact, uploadedFile2);

            var userWithNoPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _project, artifact);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => artifact.Update(userWithNoPermissions, artifact.Artifact),
                "Updating artifact should return 403 Forbidden if user doesn't have permissions to artifact!");

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("You do not have permission to access the artifact (ID: {0})", artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, expectedExceptionMessage);

            Assert.IsNotNull(Helper.FileStore.GetSQLExpiredTime(uploadedFile2.Guid), "ExpiredTime for file should not be null if artifact is not saved!");
        }

        #endregion 403 Forbidden

        #region Private functions

        /// <summary>
        /// Attaches specified file to the specified artifact and check that artifact has expected details.
        /// </summary>
        /// <param name="user">The user to perform an operation.</param>
        /// <param name="artifact">The artifact to update.</param>
        /// <param name="file">The file to attach to document.</param>
        private void UpdateDocumentFile_CanGetAttachment(IUser user, DocumentArtifactWrapper artifact, INovaFile file)
        {
            // Setup:
            CreateAndPopulateDocumentFileValue(artifact, file);

            // Execute:
            Assert.DoesNotThrow(() => artifact.Update(user, artifact.Artifact),
                "Exception caught while trying to update an artifact!");

            var updatedArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id);

            // Verify:
            Assert.IsNull(Helper.FileStore.GetSQLExpiredTime(file.Guid), "After saving ExpiredTime for file should be Null.");

            DocumentHasExpectedAttachment(artifact, updatedArtifactDetails);

            ArtifactStoreHelper.VerifyIndicatorFlags(
                Helper, user, updatedArtifactDetails.Id, ItemIndicatorFlags.HasAttachmentsOrDocumentRefs);
        }

        /// <summary>
        /// Deletes attached file from the specified Document artifact.
        /// </summary>
        /// <param name="user">The user to perform an operation.</param>
        /// <param name="artifact">The artifact to delete attachment.</param>
        private void DeleteDocumentFile_CheckAttachmentIsEmpty(IUser user, DocumentArtifactWrapper artifact)
        {
            // Setup:
            artifact.DocumentFile = null;

            // Execute:
            Assert.DoesNotThrow(() => artifact.Update(user, artifact.Artifact),
                "Exception caught while trying to update an artifact!");

            var updatedArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id);

            // Verify:
            Assert.IsNull(updatedArtifactDetails.SpecificPropertyValues[0].CustomPropertyValue, "File information should not be existing.  Value should be null!");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, user, updatedArtifactDetails.Id, expectedIndicatorFlags: null);
        }

        /// <summary>
        /// Check that artifactDetails has info about expectedDocumentFile.
        /// </summary>
        /// <param name="expectedArtifactDetails">Expected artifact details.</param>
        /// <param name="actualArtifactDetails">Artifact details to check.</param>
        private static void DocumentHasExpectedAttachment(INovaArtifactDetails expectedArtifactDetails, INovaArtifactDetails actualArtifactDetails)
        {
            var expectedDocumentFileProperty = (DocumentFileValue)expectedArtifactDetails.SpecificPropertyValues[0].CustomPropertyValue;

            var documentFileProperty = actualArtifactDetails.SpecificPropertyValues[0].CustomPropertyValue;
            var actualDocumentFile = JsonConvert.DeserializeObject<DocumentFileValue>(documentFileProperty.ToString());

            Assert.AreEqual(expectedDocumentFileProperty.FileExtension, actualDocumentFile.FileExtension, "FileExtension should have expected value, but it doesn't.");
            Assert.AreEqual(expectedDocumentFileProperty.FileName, actualDocumentFile.FileName, "FileName should have expected value, but it doesn't.");
        }

        /// <summary>
        /// Creates and populates document file value.
        /// </summary>
        /// <param name="artifact">Artifact with document attachment</param>
        /// <param name="file">File attached to artifact</param>
        /// <returns>The Document you passed in (with the file attached).</returns>
        private static DocumentArtifactWrapper CreateAndPopulateDocumentFileValue(DocumentArtifactWrapper artifact, INovaFile file)
        {
            const string EXTENTION = "xls";

            var testFileValue = new DocumentFileValue();

            testFileValue.FileExtension = EXTENTION;
            testFileValue.FileGuid = file.Guid;
            testFileValue.FileName = file.FileName;
            testFileValue.FilePath = file.UriToFile.OriginalString;
            artifact.DocumentFile = testFileValue;

            return artifact;
        }

        /// <summary>
        /// Creates a new (unpublished) Document artifact.
        /// </summary>
        /// <param name="user">The user who will create the artifact.</param>
        /// <param name="project">The project where the artifact will be created.</param>
        /// <returns>The new Document artifact (wrapped in a DocumentArtifactWrapper).</returns>
        private DocumentArtifactWrapper CreateDocumentArtifact(IUser user, IProject project)
        {
            var artifact = Helper.CreateNovaArtifact(user, project, ItemTypePredefined.Document);

            return new DocumentArtifactWrapper(artifact.Artifact, Helper.ArtifactStore, Helper.SvcShared, project, user);
        }

        /// <summary>
        /// Creates and published a new Document artifact.
        /// </summary>
        /// <param name="user">The user who will create the artifact.</param>
        /// <param name="project">The project where the artifact will be created.</param>
        /// <returns>The new Document artifact (wrapped in a DocumentArtifactWrapper).</returns>
        private DocumentArtifactWrapper CreateAndPublishDocumentArtifact(IUser user, IProject project)
        {
            var artifact = CreateDocumentArtifact(user, project);
            artifact.Publish(user);

            return artifact;
        }

        #endregion Private funactions
    }
}