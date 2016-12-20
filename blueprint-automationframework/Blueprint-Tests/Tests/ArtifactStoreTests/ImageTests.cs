using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [Explicit(IgnoreReasons.UnderDevelopmentDev)]   // Ignore all tests in this class until development is done.
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ImageTests : TestBase
    {
        private const string ADD_IMAGE_PATH = RestPaths.Svc.ArtifactStore.IMAGES;
        private const string GET_IMAGE_PATH = RestPaths.Svc.ArtifactStore.IMAGES_id_;

        private const string JPEG = "Jpeg";
        private const string PNG = "Png";

        private static Dictionary<string, ImageFormat> ImageFormatMap = new Dictionary<string, ImageFormat>
        {
            { JPEG, ImageFormat.Jpeg },
            { PNG, ImageFormat.Png }
        };

        private IUser _adminUser = null;
        private IUser _authorUser = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);
            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region AddImage tests

        [TestCase(20, 30, JPEG, "image/jpeg")]
        [TestCase(80, 80, PNG, "image/png")]
        [TestRail(211529)]
        [Description("Upload a random image file to ArtifactStore.  Verify 201 Created is returned and that the image is saved in the database properly.")]
        public void AddImage_ValidImage_ImageIsAddedToDatabase(int width, int height, string imageFormatName, string contentType)
        {
            // Setup:
            byte[] imageBytes = ImageUtilities.GenerateRandomImage(width, height, ImageFormatMap[imageFormatName]);
            string filename = I18NHelper.FormatInvariant("random-file.{0}", imageFormatName);
            var imageFile = FileFactory.CreateFile(filename, contentType, DateTime.Now, imageBytes);

            IFile returnedFile = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                returnedFile = Helper.ArtifactStore.AddImage(_authorUser, imageFile);
            }, "'POST {0}' should return 201 Created when called with a valid token & supported image format!", ADD_IMAGE_PATH);

            // Verify:
            FileStoreTestHelper.AssertFilesAreIdentical(imageFile, returnedFile, compareIds: false);

            // TODO: Make a SQL call to the new EmbeddedImages table in the Raptor DB and compare the GUID returned with what's in the DB...
            // Then get the FileStore file GUID from the DB and get the file from FileStore and compare against what we added.
        }

        #endregion AddImage tests
    }
}
