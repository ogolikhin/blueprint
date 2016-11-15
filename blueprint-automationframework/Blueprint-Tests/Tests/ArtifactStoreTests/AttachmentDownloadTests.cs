using System;
using System.Collections.Generic;
using Common;
using Helper;
using Model;
using NUnit.Framework;
using Utilities;
using CustomAttributes;
using TestCommon;
using Utilities.Factories;
using Model.ArtifactModel;
using Model.Factories;
using Model.ArtifactModel.Impl;
using Model.NovaModel;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class AttachmentDownloadTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _viewerUser = null;
        private IProject _project = null;
        private uint _fileSize = (uint)RandomGenerator.RandomNumber(4096);
        private string _fileName = null;
        private IFile _attachmentFile = null;

        private const string _fileType = "text/plain";
        //private INovaFile _novaAttachmentFile = null;
        //private DateTime defaultExpireTime = DateTime.Now.AddDays(2);//Currently Nova set ExpireTime 2 days from today for newly uploaded file

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            _project = ProjectFactory.GetProject(_adminUser);
            _fileName = I18NHelper.FormatInvariant("{0}.{1}", RandomGenerator.RandomAlphaNumeric(10), "txt");
            _attachmentFile = FileStoreTestHelper.CreateFileWithRandomByteArray(_fileSize, _fileName, "text/plain");
          //  _novaAttachmentFile = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime,
          //      Helper.FileStore);
            _viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
            _attachmentFile = null;
        }

        [TestCase]
        [TestRail(1)]
        [Description("")]
        public void GetAttachmentFile_PublishedArtifactWithAttachment_FileIsReturned()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _adminUser, BaseArtifactType.Actor);
            artifact.AddArtifactAttachment(_attachmentFile, _adminUser);
            artifact.Publish();

            Attachments attachment = null;
            attachment = Helper.ArtifactStore.GetAttachments(artifact, _viewerUser);
            int fileId = attachment.AttachedFiles[0].AttachmentId;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                Helper.ArtifactStore.GetAttachmentFile(_viewerUser, artifact.Id, fileId);
            }, "Getting attached file shouldn't return any error.");

            // Verify:
        }
    }
}
