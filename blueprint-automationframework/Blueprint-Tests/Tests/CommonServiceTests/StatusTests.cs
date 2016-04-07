using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using Utilities;

namespace CommonServiceTests
{
    public class StatusTests
    {
        private readonly IBlueprintServer _blueprintSite = BlueprintServerFactory.GetBlueprintServerFromTestConfig();

        [TestCase]
        [System.ComponentModel.Description("Calls the /status endpoint for the main Blueprint site with a valid preAuthorizedKey and verifies that it returns 200 OK and returns the proper data content.")]
        public void Status_ValidateReturnedContent()
        {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                content = _blueprintSite.GetStatus();
            }, "The GET /status endpoint should return 200 OK!");

            CommonServiceHelper.ValidateStatusResponseContent(content);
        }

        [TestCase(null)]
        [TestCase("ABCDEFG123456")]
        [System.ComponentModel.Description("Calls the /status endpoint for the main Blueprint site and passes invalid preAuthorizedKey values.  Verifies that it returns a 401 error.")]
        public void StatusWithBadKeys_Expect401Unauthorized(string preAuthorizedKey)
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _blueprintSite.GetStatus(preAuthorizedKey);
            }, "The GET /status endpoint should return 401 Unauthorized when we pass an invalid or missing preAuthorizedKey!");
        }
    }
}
