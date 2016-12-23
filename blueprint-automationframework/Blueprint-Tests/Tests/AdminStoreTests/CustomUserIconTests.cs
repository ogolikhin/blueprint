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
            IUser user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute:
            Assert.DoesNotThrow(() => Helper.AdminStore.GetCustomUserIcon(user.Id, _user, new List<HttpStatusCode> { HttpStatusCode.NoContent }),
                "'GET {0}' should return 204 No Content when user has no custom icon in his/her profile.", SVC_PATH);
        }

        [TestCase(ImageType.JPEG, "image/jpeg")]
        [TestCase(ImageType.PNG, "image/png")]
        [TestRail(211541)]
        [Description("Create user with generated custom icon. Get the user icon. Verify returned 200 OK and icon is the same as saved in database")]
        public void CustomUserIcon_GetUserIcon_ReturnsIcon(ImageType imageType, string contentType)
        {
            // Setup:
            const int WIDTH = 480;
            const int HEIGHT = 640;

            IUser user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            byte[] imageBytes = ImageUtilities.GenerateRandomImage(width: WIDTH, height: HEIGHT, imageFormat: ImageFormatMap[imageType]);

            string query = "INSERT INTO [Blueprint].[dbo].[Images] (Content) VALUES (@Content)";
            int rowsAffected = ExecuteInsertBinarySqlQuery(query, imageBytes);
            Assert.IsTrue(rowsAffected == 1, "The record was not inserted!");

            query = "SELECT ImageId FROM [Blueprint].[dbo].[Images] WHERE Content = @Content";
            int imageId = ExecuteSelectBinarySqlQuery(query, imageBytes);
            Assert.IsTrue(imageId > 0, "The record was not inserted!");

            string selectQuery = I18NHelper.FormatInvariant("UPDATE [dbo].[Users] SET Image_ImageId = {0} WHERE UserId = {1}", imageId, user.Id);
            rowsAffected = ExecuteUpdateBinarySqlQuery(selectQuery);
            Assert.IsTrue(rowsAffected == 1, "Updated more than one raw in Users table!");

            // Execute:
            IFile iconFile = null;

            Assert.DoesNotThrow(() => iconFile = Helper.AdminStore.GetCustomUserIcon(user.Id, _user),
                "'GET {0}' should return 200 OK when user has custom icon in his/her profile.", SVC_PATH);

            // Verify:
            IFile returnedFile = FileFactory.CreateFile("tmp", contentType, DateTime.Now, imageBytes);
            returnedFile.FileName = null;

            FileStoreTestHelper.AssertFilesAreIdentical(iconFile, returnedFile);
        }

        [Category(Categories.GoldenData)]
        [TestCase(1005)]
        [TestRail(211542)]
        [Description("Create user with generated custom icon. Get the user icon. Verify returned 200 OK and icon is the same as saved in database")]
        public void CustomUserIcon_GetUserIconOfDeletedUser_ReturnsIcon(int userId)
        {
            // Execute:
            IFile iconFile = null;

            Assert.DoesNotThrow(() => iconFile = Helper.AdminStore.GetCustomUserIcon(userId, _user),
                "'GET {0}' should return 200 OK when user has custom icon in his/her profile.", SVC_PATH);

            // Verify
            Assert.IsNotNull(iconFile, "The file is not returned!");
        }

        #region Private functions

        /// <summary>
        /// Executes insert binary data into raw and verifies the raw was inserted
        /// </summary>
        /// <param name="insertQuery">SQL query to insert data</param>
        /// <param name="value">Actual binary data to insert</param>
        /// <returns>Amount of records inserted<</returns>
        public static int ExecuteInsertBinarySqlQuery(string insertQuery, byte[] value)
        {
            using (var database = DatabaseFactory.CreateDatabase())
            {
                database.Open();

                Logger.WriteDebug("Running: {0}", insertQuery);

                using (var cmd = database.CreateSqlCommand(insertQuery))
                {
                    SqlParameter param = cmd.Parameters.Add("@Content", SqlDbType.VarBinary);
                    param.Value = value;

                    cmd.ExecuteNonQuery();

                    using (var sqlDataReader = cmd.ExecuteReader())
                    {
                        if (sqlDataReader.RecordsAffected <= 0)
                        {
                            throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were inserted when running: {0}", insertQuery));
                        }
                        return sqlDataReader.RecordsAffected;
                    }
                }
            }
        }

        /// <summary>
        /// Executes select query using binary content to find out image id
        /// </summary>
        /// <param name="selectQuery">SQL select query</param>
        /// <param name="content">Binary content to insert</param>
        /// <returns>Image id</returns>
        public static int ExecuteSelectBinarySqlQuery(string selectQuery, byte[] content)
        {
            using (var database = DatabaseFactory.CreateDatabase())
            {
                database.Open();

                Logger.WriteDebug("Running: {0}", selectQuery);

                using (var cmd = database.CreateSqlCommand(selectQuery))
                {
                    SqlParameter param = cmd.Parameters.Add("@Content", SqlDbType.VarBinary);
                    param.Value = content;

                    cmd.ExecuteNonQuery();

                    using (var sqlDataReader = cmd.ExecuteReader())
                    {
                        if (sqlDataReader.Read())
                        {
                            return DatabaseUtilities.GetValueOrDefault<int>(sqlDataReader, "ImageId");
                        }
                        throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were found when running: {0}", selectQuery));
                    }
                }
            }
        }

        /// <summary>
        /// Executes update query and returns amount of raw affected
        /// </summary>
        /// <param name="updatetQuery">SQL select query</param>
        /// <returns>Amount of records affected</returns>
        public static int ExecuteUpdateBinarySqlQuery(string updateQuery)
        {
            using (var database = DatabaseFactory.CreateDatabase())
            {
                database.Open();

                Logger.WriteDebug("Running: {0}", updateQuery);

                using (var cmd = database.CreateSqlCommand(updateQuery))
                {
                    cmd.ExecuteNonQuery();

                    using (var sqlDataReader = cmd.ExecuteReader())
                    {
                        if (sqlDataReader.RecordsAffected <= 0)
                        {
                            throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were inserted when running: {0}", updateQuery));
                        }
                        return sqlDataReader.RecordsAffected;
                    }
                }
            }
        }
        #endregion Private function

    }
}