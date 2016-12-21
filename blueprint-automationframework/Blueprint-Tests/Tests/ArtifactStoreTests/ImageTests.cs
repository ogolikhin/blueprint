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
using Utilities.Factories;

namespace ArtifactStoreTests
{
    [Explicit(IgnoreReasons.UnderDevelopmentDev)]   // Ignore all tests in this class until development is done.
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ImageTests : TestBase
    {
        private const string ADD_IMAGE_PATH = RestPaths.Svc.ArtifactStore.IMAGES;
        private const string GET_IMAGE_PATH = RestPaths.Svc.ArtifactStore.IMAGES_id_;

        private static Dictionary<ImageType, ImageFormat> ImageFormatMap = new Dictionary<ImageType, ImageFormat>
        {
            { ImageType.JPEG, ImageFormat.Jpeg },
            { ImageType.PNG, ImageFormat.Png }
        };

        private IUser _adminUser = null;
        private IUser _authorUser = null;
        private IProject _project = null;

        public enum ImageType
        {
            JPEG,
            PNG
        }

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

        [TestCase(20, 30, ImageType.JPEG, "image/jpeg")]
        [TestCase(80, 80, ImageType.PNG, "image/png")]
        [TestRail(211529)]
        [Description("Upload a random image file to ArtifactStore.  Verify 201 Created is returned and that the image is saved in the database properly.")]
        public void AddImage_ValidImage_ImageIsAddedToDatabase(int width, int height, ImageType imageType, string contentType)
        {
            // Setup:
            var imageFile = CreateRandomImageFile(width, height, imageType, contentType);

            IFile returnedFile = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                returnedFile = Helper.ArtifactStore.AddImage(_authorUser, imageFile);
            }, "'POST {0}' should return 201 Created when called with a valid token & supported image format!", ADD_IMAGE_PATH);

            // Verify:
            FileStoreTestHelper.AssertFilesAreIdentical(imageFile, returnedFile);

            // TODO: Make a SQL call to the new EmbeddedImages table in the Raptor DB and compare the GUID returned with what's in the DB...
            // Then get the FileStore file GUID from the DB and get the file from FileStore and compare against what we added.
        }

        [TestCase(20, 30, ImageType.JPEG, "text/plain")]
        [TestCase(80, 80, ImageType.PNG, "application/json")]
        [TestRail(211536)]
        [Description("Try to upload a random image file to ArtifactStore but use the wrong Content-Type.  Verify 400 Bad Request is returned.")]
        public void AddImage_ValidImage_InvalidContentType_400BadRequest(int width, int height, ImageType imageType, string contentType)
        {
            // Setup:
            var imageFile = CreateRandomImageFile(width, height, imageType, contentType);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.AddImage(_authorUser, imageFile);
            }, "'POST {0}' should return 400 Bad Request when called with a Content-Type of '{1}'!", ADD_IMAGE_PATH, contentType);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters,
                "TODO: Fill this in when development is done.");

            // TODO: Make a SQL call to the new EmbeddedImages table in the Raptor DB and verify the file was NOT added.
        }

        [TestCase("jpg", "image/jpeg")]
        [TestCase("png", "image/png")]
        [TestRail(211537)]
        [Description("Try to upload a random non-image file to ArtifactStore but use a valid image Content-Type and file extension.  Verify 400 Bad Request is returned.")]
        public void AddImage_ValidImageFileExtensionAndContentType_NonImageData_400BadRequest(string fileExtension, string contentType)
        {
            // Setup:
            string randomName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            string filename = I18NHelper.FormatInvariant("{0}.{1}", randomName, fileExtension);
            var nonImageFile = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize: 1024, fakeFileName: filename, fileType: contentType);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.AddImage(_authorUser, nonImageFile);
            }, "'POST {0}' should return 400 Bad Request when called with data that's not in JPEG or PNG format!", ADD_IMAGE_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters,
                "TODO: Fill this in when development is done.");

            // TODO: Make a SQL call to the new EmbeddedImages table in the Raptor DB and verify the file was NOT added.
        }

        [TestCase(5000, 10000, ImageType.JPEG, "image/jpeg")]   // Approx. 28MB
        [TestCase(1000, 10000, ImageType.PNG, "image/png")]     // Approx. 28MB
        [TestRail(211538)]
        [Description("Try to upload a random image file to ArtifactStore that exceeds the FileStore size limit.  Verify 409 Conflict is returned.")]
        public void AddImage_ValidImage_ExceedsFileSizeLimit_409Conflict(int width, int height, ImageType imageType, string contentType)
        {
            // Setup:
            var imageFile = CreateRandomImageFile(width, height, imageType, contentType);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.ArtifactStore.AddImage(_authorUser, imageFile);
            }, "'POST {0}' should return 409 Conflict when called with images that exceed the FileStore size limit!", ADD_IMAGE_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters,
                "TODO: Fill this in when development is done.");

            // TODO: Make a SQL call to the new EmbeddedImages table in the Raptor DB and verify the file was NOT added.
        }

        #endregion AddImage tests

        #region GetImage tests

        [TestCase(60, 40, ImageType.JPEG, "image/jpeg")]
        [TestCase(70, 50, ImageType.PNG, "image/png")]
        [TestRail(211535)]
        [Description("Upload a random image file to ArtifactStore, then try to get that file.  Verify 200 OK is returned by the GET call " +
            "and the same image that was uploaded is returned.")]
        public void GetImage_AddedImage_ReturnsImage(int width, int height, ImageType imageType, string contentType)
        {
            // Setup:
            var imageFile = CreateRandomImageFile(width, height, imageType, contentType);

            IFile addedFile = Helper.ArtifactStore.AddImage(_authorUser, imageFile);
            IFile returnedFile = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                returnedFile = Helper.ArtifactStore.GetImage(addedFile.Guid);
            }, "'GET {0}' should return 200 OK when a valid image GUID is passed!", GET_IMAGE_PATH);

            // Verify:
            FileStoreTestHelper.AssertFilesAreIdentical(imageFile, returnedFile);
        }

        #endregion GetImage tests

        #region Private functions

        /// <summary>
        /// Creates a random image file of the specified type and size.
        /// </summary>
        /// <param name="width">The image width.</param>
        /// <param name="height">The image height.</param>
        /// <param name="imageType">The type of image to create (ex. jpeg, png).</param>
        /// <param name="contentType">The MIME Content-Type.</param>
        /// <returns>The random image file.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]  // I want lowercase, not uppercase!
        private static IFile CreateRandomImageFile(int width, int height, ImageType imageType, string contentType)
        {
            byte[] imageBytes = ImageUtilities.GenerateRandomImage(width, height, ImageFormatMap[imageType]);
            string randomName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            string filename = I18NHelper.FormatInvariant("{0}.{1}", randomName, imageType.ToStringInvariant().ToLowerInvariant());

            return FileFactory.CreateFile(filename, contentType, DateTime.Now, imageBytes);
        }

        #endregion Private functions
    }
}
