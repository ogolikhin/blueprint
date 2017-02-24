using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel.Impl;
using NUnit.Framework;
using TestCommon;

namespace ArtifactStoreTests
{
    public class BaselineTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _viewerUser = null;
        private IProject _projectCustomData = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            _viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projectCustomData);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region Custom Data Tests
        [Category(Categories.CustomData)]
        [TestCase()]
        [TestRail(246581)]
        [Description("Get baseline by id from Custom Data project, check that Baseline has expected values.")]
        public void GetBaseline_ExistingBaseline_ValidateReturnedBaseline()
        {
            // Setup:
            const int baselineId = 83;
            const int expectedArtifactsNumber = 2;
            var expectedBaseline = new Baseline(isAvailableInAnalytics: false, notAllArtifactsAreShown: false,
                isSealed: false);

            // Execute: 
            Baseline baseline = null;
            Assert.DoesNotThrow(() => baseline = Helper.ArtifactStore.GetBaseline(_viewerUser, baselineId),
                "Get Baseline shouldn't return an error.");

            // Verify:
            Baseline.AssertBaselinesAreEqual(expectedBaseline, baseline);
            Assert.AreEqual(expectedArtifactsNumber, baseline.Artifacts.Count, "Baseline should have expected number of Artifacts.");
        }
        #endregion
    }
}
