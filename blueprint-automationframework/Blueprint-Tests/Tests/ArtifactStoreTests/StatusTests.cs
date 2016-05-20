using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    [Explicit(IgnoreReasons.DeploymentNotReady)]
    public class StatusTests
    {
        private readonly IArtifactStore _adminStore = ArtifactStoreFactory.GetArtifactStoreFromTestConfig();

        [TestCase]
        [TestRail(106928)]
        [Explicit(IgnoreReasons.ProductBug)]  // ArtifactStore is still being written...
        [Description("Calls the /status endpoint for ArtifactStore with a valid preAuthorizedKey and verifies that it returns 200 OK and returns the proper data content.")]
        public void Status_ValidateReturnedContent()
        {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                content = _adminStore.GetStatus();
            }, "The GET /status endpoint should return 200 OK!");

            var extraExpectedStrings = new List<string> { "ArtifactStore" };

            CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
        }

        [TestCase(null)]
        [TestCase("ABCDEFG123456")]
        [TestRail(106929)]
        [Explicit(IgnoreReasons.ProductBug)]  // ArtifactStore is still being written...
        [Description("Calls the /status endpoint for ArtifactStore and passes invalid preAuthorizedKey values.  Verifies that it returns a 401 error.")]
        public void StatusWithBadKeys_Expect401Unauthorized(string preAuthorizedKey)
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _adminStore.GetStatus(preAuthorizedKey);
            }, "The GET /status endpoint should return 401 Unauthorized when we pass an invalid or missing preAuthorizedKey!");
        }

        [TestCase]
        [TestRail(106930)]
        [Description("Calls the /status/upcheck endpoint for ArtifactStore and verifies that it returns 200 OK.")]
        public void GetStatusUpcheck_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                _adminStore.GetStatusUpcheck();
            });
        }
    }
}
