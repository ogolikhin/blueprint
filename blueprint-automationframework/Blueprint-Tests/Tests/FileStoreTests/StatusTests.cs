using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using Utilities;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.FileStore)]
    public class StatusTests
    {
        private readonly IFileStore _filestore = FileStoreFactory.GetFileStoreFromTestConfig();

        [TestCase]
        [TestRail(106953)]
        [Description("Calls the /status endpoint for FileStore with a valid preAuthorizedKey and verifies that it returns 200 OK and a JSON structure containing the status of dependent services.")]
        public void Status_ValidateReturnedContent()
        {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                content = _filestore.GetStatus();
            }, "The GET /status endpoint should return 200 OK!");

            var extraExpectedStrings = new List<string> { "FileStore", "FileStorageDB" };

            CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
        }

        [TestCase(null)]
        [TestCase("ABCDEFG123456")]
        [TestRail(106954)]
        [Description("Calls the /status endpoint for FileStore and passes invalid preAuthorizedKey values.  Verifies that it returns a 401 error.")]
        public void StatusWithBadKeys_Expect401Unauthorized(string preAuthorizedKey)
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _filestore.GetStatus(preAuthorizedKey);
            }, "The GET /status endpoint should return 401 Unauthorized when we pass an invalid or missing preAuthorizedKey!");
        }

        [TestCase]
        [TestRail(106955)]
        [Description("Calls the /status/upcheck endpoint for FileStore and verifies that it returns 200 OK.")]
        public void StatusUpcheck_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                _filestore.GetStatusUpcheck();
            });
        }
    }
}
