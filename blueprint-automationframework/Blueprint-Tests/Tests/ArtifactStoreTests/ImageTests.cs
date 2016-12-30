using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
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
            { ImageType.PNG, ImageFormat.Png },
            { ImageType.GIF, ImageFormat.Gif },
            { ImageType.TIFF, ImageFormat.Tiff }
        };

        private IUser _adminUser = null;
        private IUser _authorUser = null;
        private IProject _project = null;

        public enum ImageType
        {
            JPEG,
            PNG,
            GIF,
            TIFF
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
            Assert.NotNull(returnedFile, "AddImage() shouldn't return null if successful!");
            FileStoreTestHelper.AssertFilesAreIdentical(imageFile, returnedFile);

            // Get the file from FileStore and compare against what we uploaded.
            var fileStoreFileId = returnedFile.Guid;
            var filestoreFile = Helper.FileStore.GetFile(fileStoreFileId, _adminUser);

            FileStoreTestHelper.AssertFilesAreIdentical(returnedFile, filestoreFile);

//            DeleteFileFromDB(returnedFile.Guid);
        }

        [TestCase(20, 30, ImageType.GIF, "image/gif")]
        [TestCase(80, 80, ImageType.TIFF, "image/tiff")]
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
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ImageTypeNotSupported,
                "Specified image type isn't supported.");

            AssertFileNotInEmbeddedImagesTable(imageFile.FileName);
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
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ImageTypeNotSupported,
                "Specified image type isn't supported.");

            AssertFileNotInEmbeddedImagesTable(nonImageFile.FileName);
        }

        [TestCase(20, 30, ImageType.JPEG, "image/jpeg", "00000000-0000-0000-0000-000000000000")]
        [TestCase(80, 80, ImageType.PNG, "image/png", "")]
        [TestCase(50, 50, ImageType.PNG, "image/png", null)]
        [TestRail(211547)]
        [Description("Try to upload a random image file to ArtifactStore but use an invalid or missing token.  Verify 401 Unauthorized is returned.")]
        public void AddImage_InvalidToken_401Unauthorized(int width, int height, ImageType imageType, string contentType, string token)
        {
            // Setup:
            var imageFile = CreateRandomImageFile(width, height, imageType, contentType);

            // Set the bad token.
            _authorUser.Token.AccessControlToken = token;

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.AddImage(_authorUser, imageFile);
            }, "'POST {0}' should return 401 Unauthorized when called with an invalid token!", ADD_IMAGE_PATH);

            // Verify:
            string expectedMessage = "Unauthorized call";
            Assert.That(ex.RestResponse.Content.Contains(expectedMessage), "{0} should be found when no image id is provided!", expectedMessage);

            AssertFileNotInEmbeddedImagesTable(imageFile.FileName);
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
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ExceedsLimit,
                "Specified image size is over limit.");

            AssertFileNotInEmbeddedImagesTable(imageFile.FileName);
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

            string imageGuid = GetImageGuidFromFileGuid(addedFile.Guid);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                returnedFile = Helper.ArtifactStore.GetImage(imageGuid);
            }, "'GET {0}' should return 200 OK when a valid image GUID is passed!", GET_IMAGE_PATH);

            // Verify:
            FileStoreTestHelper.AssertFilesAreIdentical(imageFile, returnedFile, compareFileName: false);
        }

        [TestCase("abcd1234")]
        [TestRail(211550)]
        [Description("Try to get an image with an malformed ImageId GUID.  Verify it returns 404 Not Found.")]
        public void GetImage_MalformedImageGuid_400BadRequest(string imageId)
        {
            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.GetImage(imageId);
            }, "'GET {0}' should return 400 Bad request when bad GUID is passed!", GET_IMAGE_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, "Invalid format of specified image id.");
        }

        [TestCase("")]
        [TestRail(213022)]
        [Description("Try to get an image with no ImageId specified.  Verify it returns 404 Not Found.")]
        public void GetImage_NonImageIdSpecified_404NotFound(string imageId)
        {
            // Execute:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetImage(imageId);
            }, "'GET {0}' should return 404 Not Found when passed image GUID for non existing image!", GET_IMAGE_PATH);

        }

        [TestCase("00000000-0000-0000-0000-000000000000")]
        [TestRail(213039)]
        [Description("Try to get an image with no ImageId specified.  Verify it returns 404 Not Found.")]
        public void GetImage_NonExistingImage_404NotFound(string imageId)
        {
            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetImage(imageId);
            }, "'GET {0}' should return 404 Not Found when a image GUID not provided!", GET_IMAGE_PATH);

            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound, "The image with the given GUID does not exist");
        }

        #endregion GetImage tests

        #region Private functions

        /// <summary>
        /// Asserts that the EmbeddedImages table contains no rows with the specified filename.
        /// </summary>
        /// <param name="filename">The filename to look for.</param>
        private static void AssertFileNotInEmbeddedImagesTable(string filename)
        {
            string selectQuery = I18NHelper.FormatInvariant("SELECT COUNT(*) FROM [Blueprint_FileStorage].[FileStore].[Files] WHERE [FileName] ='{0}'", filename);
            int numberOfRows = DatabaseHelper.ExecuteSingleValueSqlQuery<int>(selectQuery, "FileId");

            Assert.AreEqual(0, numberOfRows,
                "Found {0} rows in the EmbeddedImages table containing FileName: '{1}'", numberOfRows, filename);
        }

        /// <summary>
        /// Finds image GUID by file GUID
        /// </summary>
        /// <param name="fileGuid">File GUID to find image GUID</param>
        /// <returns>Image GUID</returns>
        private static string GetImageGuidFromFileGuid(string fileGuid)
        {
            string selectQuery = I18NHelper.FormatInvariant("SELECT EmbeddedImageId FROM [Blueprint].[dbo].[EmbeddedImages] WHERE [FileId] ='{0}'", fileGuid);
            string imageGuid = DatabaseHelper.ExecuteSingleValueSqlQuery<string>(selectQuery, "EmbeddedImageId");

            Assert.IsNotNullOrEmpty(imageGuid, "Image GUID cannot be found in EmbeddedImages table!");

            return imageGuid;
        }

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
/*
        private static int DeleteFileFromDB(string fileGuid)
        {
            string selectQuery = I18NHelper.FormatInvariant("DELETE FROM [Blueprint_FileStorage].[FileStore].[Files] WHERE [FileId] ='{0}'", fileGuid);
            int rowsAffected = DatabaseHelper.ExecuteSingleValueSqlQuery<int>(selectQuery, "FileId");

            Assert.IsNotNull(rowsAffected, "Image GUID cannot be found in EmbeddedImages table!");

            return rowsAffected;
        }*/

        #endregion Private functions
    }
}
