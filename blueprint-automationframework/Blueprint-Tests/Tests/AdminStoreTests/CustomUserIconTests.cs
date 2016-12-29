using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Net;
using TestCommon;
using Utilities;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class CustomUserIconTests : TestBase
    {
        private IProject _project = null;
        private IUser _user = null;

        private static Dictionary<ImageType, ImageFormat> ImageFormatMap = new Dictionary<ImageType, ImageFormat>
        {
            { ImageType.JPEG, ImageFormat.Jpeg },
            { ImageType.PNG, ImageFormat.Png }
        };

        public enum ImageType
        {
            JPEG,
            PNG
        }

        private const string SVC_PATH = RestPaths.Svc.AdminStore.Users_id_.ICON;

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
        [TestRail(211540)]
        [Description("Create a user without a custom icon. Get the user icon. Verify 204 No Content with empty body returned")]
        public void GetCustomUserIcon_NoIconExistsForThisUser_204NoContent()
        {
            // Setup:
            IUser viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute:
            Assert.DoesNotThrow(() => Helper.AdminStore.GetCustomUserIcon(viewerUser.Id, viewerUser, new List<HttpStatusCode> { HttpStatusCode.NoContent }),
                "'GET {0}' should return 204 No Content when user has no custom icon in his/her profile.", SVC_PATH);
        }

        [TestCase(ImageType.JPEG, "image/jpeg")]
        [TestCase(ImageType.PNG, "image/png")]
        [TestRail(211541)]
        [Description("Create user with generated custom icon. Get the user icon. Verify returned 200 OK and icon is the same as saved in database")]
        public void GetCustomUserIcon_ReturnsIcon(ImageType imageType, string contentType)
        {
            // Setup:
            IUser viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            byte[] imageBytes = ImageUtilities.GenerateRandomImage(width: 480, height: 640, imageFormat: ImageFormatMap[imageType]);

            viewerUser.SetUserIcon(viewerUser.Id, imageBytes);

            // Execute:
            IFile iconFile = null;

            Assert.DoesNotThrow(() => iconFile = Helper.AdminStore.GetCustomUserIcon(viewerUser.Id, _user),
                "'GET {0}' should return 200 OK when user has custom icon in his/her profile.", SVC_PATH);

            // Verify:
            IFile expectedFile = FileFactory.CreateFile("tmp", contentType, DateTime.Now, imageBytes);
            expectedFile.FileName = null;

            FileStoreTestHelper.AssertFilesAreIdentical(expectedFile, iconFile);
        }

        [TestCase(ImageType.JPEG, "image/jpeg")]
        [TestCase(ImageType.PNG, "image/png")]
        [TestRail(211542)]
        [Description("Create user with generated custom icon. Delete user and get the user icon. Verify returned 200 OK and icon is the same as saved in database")]
        public void GetCustomUserIcon_DeletedUser_ReturnsIcon(ImageType imageType, string contentType)
        {
            // Setup:
            IUser viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            byte[] imageBytes = ImageUtilities.GenerateRandomImage(width: 480, height: 640, imageFormat: ImageFormatMap[imageType]);

            viewerUser.SetUserIcon(viewerUser.Id, imageBytes);

            viewerUser.DeleteUser();

            // Execute:
            IFile iconFile = null;

            Assert.DoesNotThrow(() => iconFile = Helper.AdminStore.GetCustomUserIcon(viewerUser.Id, viewerUser),
                "'GET {0}' should return 200 OK when user has custom icon in his/her profile.", SVC_PATH);

            // Verify:
            IFile expectedFile = FileFactory.CreateFile("tmp", contentType, DateTime.Now, imageBytes);
            expectedFile.FileName = null;

            FileStoreTestHelper.AssertFilesAreIdentical(expectedFile, iconFile);
        }

        #endregion Positive tests

        #region Negative tests

        [TestCase("00000000-0000-0000-0000-000000000000", "Token is invalid.")]
        [TestCase(null, "Token is missing or malformed.")]
        [TestRail(211707)]
        [Description("Create user with bad token.  User tries to get user icon.  Verify response returns code 401 Unauthorized.")]
        public void GetCustomUserIcon_UserWithMissingOrBadToken_401Unauthorized(string token, string expectedMessage)
        {
            // Setup:
            _user.Token.AccessControlToken = token;

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.GetCustomUserIcon(_user.Id, _user);
            }, "'GET {0}' should return 401 Unauthorized when called with an invalid token!", SVC_PATH);

            // Verify:
            Assert.That(ex.RestResponse.Content.Contains(expectedMessage), "{0} should be found when token is invalid!", expectedMessage);
        }

        [TestCase(int.MaxValue)]
        [TestRail(211709)]
        [Description("User tries to get user icon from non-existing user.  Verify response returns code 404 Not Found.")]
        public void GetCustomUserIcon_NonExistingUser_404NotFound(int nonExistingUserId)
        {
            // Setup:
            IUser viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.AdminStore.GetCustomUserIcon(nonExistingUserId, viewerUser);
            }, "'GET {0}' should return 404 Not Found when get user icon called for non-existing user!", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ResourceNotFound,
                I18NHelper.FormatInvariant("User does not exist with UserId: {0}", nonExistingUserId));
        }

        [TestCase(0)]
        [TestRail(213032)]
        [Description("User tries to get user icon from user with user id 0.  Verify response returns code 404 Not Found.")]
        public void GetCustomUserIcon_UserId0_404NotFound(int UserId0)
        {
            // Setup:
            IUser viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.AdminStore.GetCustomUserIcon(UserId0, viewerUser);
            }, "'GET {0}' should return 404 Not Found when get user icon called for user with user id 0!", SVC_PATH);
        }

        #endregion Negative tests
    }
}