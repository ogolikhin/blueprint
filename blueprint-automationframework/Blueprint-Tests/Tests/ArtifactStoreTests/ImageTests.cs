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
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ImageTests : TestBase
    {
        private const string ADD_IMAGE_PATH = RestPaths.Svc.ArtifactStore.IMAGES;
        private const string GET_IMAGE_PATH = RestPaths.Svc.ArtifactStore.IMAGES_id_;

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

        [TestCase(20, 30, ArtifactStoreHelper.ImageType.JPEG, "image/jpeg")]
        [TestCase(80, 80, ArtifactStoreHelper.ImageType.PNG, "image/png")]
        [TestRail(211529)]
        [Description("Upload a random image file to ArtifactStore.  Verify 201 Created is returned and that the image is saved in the database properly.")]
        public void AddImage_ValidImage_ImageIsAddedToDatabase(int width, int height, ArtifactStoreHelper.ImageType imageType, string contentType)
        {
            // Setup:
            var imageFile = ArtifactStoreHelper.CreateRandomImageFile(width, height, imageType, contentType);

            EmbeddedImageFile returnedFile = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                returnedFile = Helper.ArtifactStore.AddImage(_authorUser, imageFile);
            }, "'POST {0}' should return 201 Created when called with a valid token & supported image format!", ADD_IMAGE_PATH);

            // Verify:
            Assert.NotNull(returnedFile, "AddImage() shouldn't return null if successful!");
            FileStoreTestHelper.AssertFilesAreIdentical(imageFile, returnedFile);

            Assert.AreNotEqual(returnedFile.Guid, returnedFile.EmbeddedImageId, "The EmbeddedImageId should not be the same as the FileStore FileId!");

            // Get the file from FileStore and compare against what we uploaded.
            var fileStoreFileId = returnedFile.Guid;
            var filestoreFile = Helper.FileStore.GetFile(fileStoreFileId, _adminUser);

            FileStoreTestHelper.AssertFilesAreIdentical(returnedFile, filestoreFile, compareContent: false);
        }

        [TestCase(500, 500, ArtifactStoreHelper.ImageType.JPEG, "image/jpeg")]
        [TestCase(500, 300, ArtifactStoreHelper.ImageType.JPEG, "image/jpeg")]
        [TestCase(300, 600, ArtifactStoreHelper.ImageType.PNG, "image/png")]
        [TestRail(211529)]
        [Description("Upload a random image file with resolution bigger than 400*400 to ArtifactStore.  Verify 201 Created is returned and that the image is saved in the database properly, size should be reduced.")]
        public void AddImage_ValidImageWithHighResolution_ImageIsAddedToDatabase(int width, int height, ArtifactStoreHelper.ImageType imageType, string contentType)
        {
            // Setup:
            var imageFile = ArtifactStoreHelper.CreateRandomImageFile(width, height, imageType, contentType);

            EmbeddedImageFile returnedFile = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                returnedFile = Helper.ArtifactStore.AddImage(_authorUser, imageFile);
            }, "'POST {0}' should return 201 Created when called with a valid token & supported image format!", ADD_IMAGE_PATH);

            // Verify:
            Assert.NotNull(returnedFile, "AddImage() shouldn't return null if successful!");
            FileStoreTestHelper.AssertFilesAreIdentical(imageFile, returnedFile);
            
            Assert.AreNotEqual(returnedFile.Guid, returnedFile.EmbeddedImageId, "The EmbeddedImageId should not be the same as the FileStore FileId!");

            // Get the file from FileStore and compare against what we uploaded.
            var fileStoreFileId = returnedFile.Guid;
            var filestoreFile = Helper.FileStore.GetFile(fileStoreFileId, _adminUser);

            FileStoreTestHelper.AssertFilesAreIdentical(returnedFile, filestoreFile, compareContent: false);
            // TODO: add real resolution check
        }

        [TestCase(20, 30, ArtifactStoreHelper.ImageType.GIF, "image/gif")]
        [TestCase(80, 80, ArtifactStoreHelper.ImageType.TIFF, "image/tiff")]
        [TestRail(211536)]
        [Description("Try to upload a random image file to ArtifactStore but use the wrong Content-Type.  Verify 400 Bad Request is returned.")]
        public void AddImage_ValidImage_InvalidContentType_400BadRequest(int width, int height, ArtifactStoreHelper.ImageType imageType, string contentType)
        {
            // Setup:
            var imageFile = ArtifactStoreHelper.CreateRandomImageFile(width, height, imageType, contentType);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.AddImage(_authorUser, imageFile);
            }, "'POST {0}' should return 400 Bad Request when called with a Content-Type of '{1}'!", ADD_IMAGE_PATH, contentType);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ImageTypeNotSupported, "Specified image type isn't supported.");

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
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ImageTypeNotSupported, "Specified image type isn't supported.");

            AssertFileNotInEmbeddedImagesTable(nonImageFile.FileName);
        }

        [TestCase(80, 80, ArtifactStoreHelper.ImageType.PNG, "image/png")]
        [TestRail(213049)]
        [Description("Upload a random image file to ArtifactStore.  Make sure filename parameter is not set. Verify 400 Bad Request is returned.")]
        public void AddImage_ValidImageWithNotSetFileName_400BadRequest(int width, int height, ArtifactStoreHelper.ImageType imageType, string contentType)
        {
            // Setup:
            var imageFile = ArtifactStoreHelper.CreateRandomImageFile(width, height, imageType, contentType);

            imageFile.FileName = null;

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.AddImage(_authorUser, imageFile);
            }, "'POST {0}' should return 400 Bad Request when called with filename parameter set in a header as null!", ADD_IMAGE_PATH);

            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ValidationFailed, "The file name is missing or malformed.");
        }

        [TestCase(20, 30, ArtifactStoreHelper.ImageType.JPEG, "image/jpeg", CommonConstants.InvalidToken)]
        [TestCase(80, 80, ArtifactStoreHelper.ImageType.PNG, "image/png", "")]
        [TestCase(50, 50, ArtifactStoreHelper.ImageType.PNG, "image/png", null)]
        [TestRail(211547)]
        [Description("Try to upload a random image file to ArtifactStore but use an invalid or missing token.  Verify 401 Unauthorized is returned.")]
        public void AddImage_InvalidToken_401Unauthorized(int width, int height, ArtifactStoreHelper.ImageType imageType, string contentType, string token)
        {
            // Setup:
            var imageFile = ArtifactStoreHelper.CreateRandomImageFile(width, height, imageType, contentType);

            // Set the bad token.
            _authorUser.Token.AccessControlToken = token;

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.AddImage(_authorUser, imageFile);
            }, "'POST {0}' should return 401 Unauthorized when called with an invalid token!", ADD_IMAGE_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, "Unauthorized call");

            AssertFileNotInEmbeddedImagesTable(imageFile.FileName);
        }

        [TestCase(5000, 10000, ArtifactStoreHelper.ImageType.JPEG, "image/jpeg")]   // Approx. 28MB
        [TestCase(1000, 10000, ArtifactStoreHelper.ImageType.PNG, "image/png")]     // Approx. 28MB
        [TestRail(211538)]
        [Description("Try to upload a random image file to ArtifactStore that exceeds the FileStore size limit.  Verify 409 Conflict is returned.")]
        public void AddImage_ValidImage_ExceedsFileSizeLimit_409Conflict(int width, int height, ArtifactStoreHelper.ImageType imageType, string contentType)
        {
            // Setup:
            var imageFile = ArtifactStoreHelper.CreateRandomImageFile(width, height, imageType, contentType);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.ArtifactStore.AddImage(_authorUser, imageFile);
            }, "'POST {0}' should return 409 Conflict when called with images that exceed the FileStore size limit!", ADD_IMAGE_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ExceedsLimit, "Specified image size is over limit.");

            AssertFileNotInEmbeddedImagesTable(imageFile.FileName);
        }

        #endregion AddImage tests

        #region GetImage tests

        [TestCase(60, 40, ArtifactStoreHelper.ImageType.JPEG, "image/jpeg")]
        [TestCase(70, 50, ArtifactStoreHelper.ImageType.PNG, "image/png")]
        [TestRail(211535)]
        [Description("Upload a random image file to ArtifactStore, then try to get that file.  Verify 200 OK is returned by the GET call " +
            "and the same image that was uploaded is returned.")]
        public void GetImage_AddedImage_ReturnsImage(int width, int height, ArtifactStoreHelper.ImageType imageType, string contentType)
        {
            // Setup:
            var user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _project);
            var imageFile = ArtifactStoreHelper.CreateRandomImageFile(width, height, imageType, contentType);

            var addedFile = Helper.ArtifactStore.AddImage(_authorUser, imageFile);
            IFile returnedFile = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                returnedFile = Helper.ArtifactStore.GetImage(user, addedFile.EmbeddedImageId);
            }, "'GET {0}' should return 200 OK when a valid image GUID is passed!", GET_IMAGE_PATH);

            // Verify:
            FileStoreTestHelper.AssertFilesAreIdentical(imageFile, returnedFile, compareFileNames: false, compareContent: false);
        }

        [TestCase(500, 500, ArtifactStoreHelper.ImageType.JPEG, "image/jpeg")]
        [TestCase(500, 300, ArtifactStoreHelper.ImageType.JPEG, "image/jpeg")]
        [TestCase(300, 600, ArtifactStoreHelper.ImageType.PNG, "image/png")]
        [TestRail(227323)]
        [Description("Upload a random image file to ArtifactStore, then try to get that file.  Verify 200 OK is returned by the GET call " +
            "and the same image that was uploaded is returned.")]
        public void GetImage_AddedImageWithHighResolution_ReturnsImage(int width, int height, ArtifactStoreHelper.ImageType imageType, string contentType)
        {
            // Setup:
            var imageFile = ArtifactStoreHelper.CreateRandomImageFile(width, height, imageType, contentType);

            var addedFile = Helper.ArtifactStore.AddImage(_authorUser, imageFile);
            IFile returnedFile = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                returnedFile = Helper.ArtifactStore.GetImage(_authorUser, addedFile.EmbeddedImageId);
            }, "'GET {0}' should return 200 OK when a valid image GUID is passed!", GET_IMAGE_PATH);

            // Verify:
            FileStoreTestHelper.AssertFilesAreIdentical(imageFile, returnedFile, compareFileNames: false, compareContent: false);
            // TODO: add real resolution check
        }

        [TestCase("abcd1234")]
        [TestRail(211550)]
        [Description("Try to get an image with malformed ImageId GUID.  Verify it returns 400 Bad Request.")]
        public void GetImage_MalformedImageGuid_400BadRequest(string imageId)
        {
            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.GetImage(_authorUser, imageId);
            }, "'GET {0}' should return 400 Bad request when bad GUID is passed!", GET_IMAGE_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, "Invalid format of specified image id.");
        }

        [TestCase(null)]
        [TestCase(CommonConstants.InvalidToken)]
        [TestRail(227357)]
        [Description("Upload a random image file to ArtifactStore, then try to get that file but pass an invalid token.  Verify 401 Unauthorized is returned.")]
        public void GetImage_InvalidToken_401Unauthorized(string invalidToken)
        {
            // Setup:
            var imageFile = ArtifactStoreHelper.CreateRandomImageFile(width: 50, height: 50, imageType: ArtifactStoreHelper.ImageType.PNG, contentType: "image/png");

            var addedFile = Helper.ArtifactStore.AddImage(_authorUser, imageFile);

            // Invalidate the user token.
            _authorUser.Token.AccessControlToken = invalidToken;

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetImage(_authorUser, addedFile.EmbeddedImageId);
            }, "'GET {0}' should return 401 Unauthorized when an invalid token is passed!", GET_IMAGE_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, "Unauthorized call");
        }

        [TestCase("")]
        [TestRail(213022)]
        [Description("Try to get an image with no ImageId specified.  Verify it returns 404 Not Found.")]
        public void GetImage_NoImageIdSpecified_404NotFound(string imageId)
        {
            // Execute:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetImage(_authorUser, imageId);
            }, "'GET {0}' should return 404 Not Found when passed image GUID for non existing image!", GET_IMAGE_PATH);
        }

        [TestCase(CommonConstants.InvalidToken)]
        [TestRail(213039)]
        [Description("Try to get an image with no ImageId specified.  Verify it returns 404 Not Found.")]
        public void GetImage_NonExistingImage_404NotFound(string imageId)
        {
            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetImage(_authorUser, imageId);
            }, "'GET {0}' should return 404 Not Found when a image GUID not provided!", GET_IMAGE_PATH);

            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound, "The image with the given GUID does not exist");
        }

        #endregion GetImage tests

        #region Other tests

        [TestCase(60, 40, ArtifactStoreHelper.ImageType.JPEG, "image/jpeg")]
        [TestCase(70, 50, ArtifactStoreHelper.ImageType.PNG, "image/png")]
        [TestRail(227091)]
        [Description("Create & publish artifact. Upload a random image file and then update artifact with this image.  " +
                     "Verify ExpiredTime field for this image is updated to null in EmbeddedImages and Files tables.")]
        public void UpdateArtifact_AddImageToArtifact_ExpiredTimeFieldUpdatedToNull(int width, int height, ArtifactStoreHelper.ImageType imageType, string contentType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, ItemTypePredefined.Process);
            artifact.Lock(_authorUser);

            var imageFile = ArtifactStoreHelper.CreateRandomImageFile(width, height, imageType, contentType);

            var addedFile = Helper.ArtifactStore.AddImage(_authorUser, imageFile);

            string propertyContent = ArtifactStoreHelper.CreateEmbeddedImageHtml(addedFile.EmbeddedImageId);

            CSharpUtilities.SetProperty(nameof(NovaArtifactDetails.Description), propertyContent, artifact);

            // Execute:
            artifact.Update(_authorUser, artifact.Artifact);

            // Verify:
            string selectQuery = I18NHelper.FormatInvariant("SELECT ExpiredTime FROM [dbo].[EmbeddedImages] WHERE [FileId] ='{0}'", addedFile.Guid);
            Assert.IsNull(DatabaseHelper.ExecuteSingleValueSqlQuery<string>(selectQuery), "ExpiredTime is not null!");

            selectQuery = I18NHelper.FormatInvariant("SELECT ExpiredTime FROM [FileStore].[Files] WHERE [FileId] = '{0}'", addedFile.Guid);
            Assert.IsNull(DatabaseHelper.ExecuteSingleValueSqlQuery<string>(selectQuery, "FileStore"), "ExpiredTime is not null!");
        }

        [TestCase(70, 50, ArtifactStoreHelper.ImageType.PNG, "image/png")]
        [TestRail(227235)]
        [Description("Create & publish artifact.  Upload a random image file and then update artifact with this image in a Custom Rich Text Property.  " +
                     "Verify ExpiredTime field for this image is updated to null in EmbeddedImages and Files tables.")]
        public void UpdateArtifact_AddImageToCustomProperty_ExpiredTimeFieldUpdatedToNull(int width, int height, ArtifactStoreHelper.ImageType imageType, string contentType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);

            // The 'ST-Title' property of 'ST-User Story' is the oly single-line Rich Text property.
            const string artifactTypeName = "ST-User Story";
            const string multiLineRTProperty = "ST-Acceptance Criteria";

            var artifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, ItemTypePredefined.TextualRequirement,
                artifactTypeName: artifactTypeName);
            artifact.Lock(_authorUser);

            var imageFile = ArtifactStoreHelper.CreateRandomImageFile(width, height, imageType, contentType);

            var addedFile = Helper.ArtifactStore.AddImage(_authorUser, imageFile);

            string propertyContent = ArtifactStoreHelper.CreateEmbeddedImageHtml(addedFile.EmbeddedImageId);
            var customProperty = artifact.CustomPropertyValues.Find(p => p.Name == multiLineRTProperty);
            Assert.NotNull(customProperty, "Couldn't find a Custom Property named: {0}!", multiLineRTProperty);

            customProperty.CustomPropertyValue = propertyContent;

            // Execute:
            artifact.Update(_authorUser, artifact.Artifact);

            // Verify:
            string selectQuery = I18NHelper.FormatInvariant("SELECT ExpiredTime FROM [dbo].[EmbeddedImages] WHERE [FileId] ='{0}'", addedFile.Guid);
            Assert.IsNull(DatabaseHelper.ExecuteSingleValueSqlQuery<string>(selectQuery), "ExpiredTime is not null!");

            selectQuery = I18NHelper.FormatInvariant("SELECT ExpiredTime FROM [FileStore].[Files] WHERE [FileId] = '{0}'", addedFile.Guid);
            Assert.IsNull(DatabaseHelper.ExecuteSingleValueSqlQuery<string>(selectQuery, "FileStore"), "ExpiredTime is not null!");
        }

        #endregion Other tests

        #region Private functions

        /// <summary>
        /// Asserts that the EmbeddedImages table contains no rows with the specified filename.
        /// </summary>
        /// <param name="filename">The filename to look for.</param>
        private static void AssertFileNotInEmbeddedImagesTable(string filename)
        {
            string fileIdQuery = I18NHelper.FormatInvariant("SELECT COUNT(*) FROM [FileStore].[Files] WHERE [FileName] ='{0}'", filename);
            int numberOfRows = DatabaseHelper.ExecuteSingleValueSqlQuery<int>(fileIdQuery, databaseName: "FileStore");

            Assert.AreEqual(0, numberOfRows, "Found {0} rows in the EmbeddedImages table containing FileName: '{1}'", numberOfRows, filename);
        }

        #endregion Private functions
    }
}
