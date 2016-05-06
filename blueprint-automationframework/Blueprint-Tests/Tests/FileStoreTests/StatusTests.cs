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
        [Description("Calls the /status endpoint for FileStore with a valid preAuthorizedKey and verifies that it returns 200 OK and returns the proper data content.")]
        public void Status_ValidateReturnedContent()
        {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                content = _filestore.GetStatus();
            }, "The GET /status endpoint should return 200 OK!");

            CommonServiceHelper.ValidateStatusResponseContent(content);
        }

        [TestCase(null)]
        [TestCase("ABCDEFG123456")]
        [Description("Calls the /status endpoint for FileStore and passes invalid preAuthorizedKey values.  Verifies that it returns a 401 error.")]
        public void StatusWithBadKeys_Expect401Unauthorized(string preAuthorizedKey)
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _filestore.GetStatus(preAuthorizedKey);
            }, "The GET /status endpoint should return 401 Unauthorized when we pass an invalid or missing preAuthorizedKey!");
        }

        [TestCase]
        [Description("Calls the /status/upcheck endpoint for FileStore and verifies that it returns 200 OK")]
        public void StatusUpcheck_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                _filestore.GetStatusUpcheck();
            });
        }
    }
}
