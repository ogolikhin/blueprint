using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Imaging;
using System.Net;
using TestCommon;
using Utilities;
using Utilities.Factories;

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

        [TestCase()]
        [TestRail(211540)]
        [Description("Create a user without a custom icon. Get the user icon. Verify 204 No Content with empty body returned")]
        public void CustomUserIcon_GetUserIcon_NoIconExistsForThisUser_204NoContent()
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
        public void CustomUserIcon_GetUserIcon_ReturnsIcon(ImageType imageType, string contentType)
        {
            // Setup:
            IUser viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            byte[] imageBytes = ImageUtilities.GenerateRandomImage(width: 480, height: 640, imageFormat: ImageFormatMap[imageType]);

            viewerUser.setUserIcon(viewerUser.Id, imageBytes);
            
            // Execute:
            IFile iconFile = null;

            Assert.DoesNotThrow(() => iconFile = Helper.AdminStore.GetCustomUserIcon(viewerUser.Id, _user),
                "'GET {0}' should return 200 OK when user has custom icon in his/her profile.", SVC_PATH);

            // Verify:
            IFile expectedFile = FileFactory.CreateFile("tmp", contentType, DateTime.Now, imageBytes);
            expectedFile.FileName = null;

            FileStoreTestHelper.AssertFilesAreIdentical(iconFile, expectedFile);
        }

        [TestCase(ImageType.JPEG, "image/jpeg")]
        [TestCase(ImageType.PNG, "image/png")]
        [TestRail(211542)]
        [Description("Create user with generated custom icon. Get the user icon. Verify returned 200 OK and icon is the same as saved in database")]
        public void CustomUserIcon_GetUserIconOfDeletedUser_ReturnsIcon(ImageType imageType, string contentType)
        {
            // Setup:
            IUser viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            byte[] imageBytes = ImageUtilities.GenerateRandomImage(width: 480, height: 640, imageFormat: ImageFormatMap[imageType]);

            viewerUser.setUserIcon(viewerUser.Id, imageBytes);

            viewerUser.DeleteUser();

            // Execute:
            IFile iconFile = null;

            Assert.DoesNotThrow(() => iconFile = Helper.AdminStore.GetCustomUserIcon(viewerUser.Id, _user),
                "'GET {0}' should return 200 OK when user has custom icon in his/her profile.", SVC_PATH);

            // Verify:
            IFile expectedFile = FileFactory.CreateFile("tmp", contentType, DateTime.Now, imageBytes);
            expectedFile.FileName = null;

            FileStoreTestHelper.AssertFilesAreIdentical(iconFile, expectedFile);
        }
    }
}