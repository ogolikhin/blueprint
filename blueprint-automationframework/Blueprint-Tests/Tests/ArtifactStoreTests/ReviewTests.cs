using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel.Impl;
using Model.ArtifactModel.Impl.OperationsResults;
using NUnit.Framework;
using TestCommon;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ReviewTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _user = null;
        private IProject _projectCustomData = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(290224)]
        [Description("Get Review Content by id from Custom Data project, check that artifacts have expected values.")]
        public void GetReviewArtifacts_ExistingReview_ValidateReturnOK200()
        {
            // Setup:
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            _user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projectCustomData);
            const int reviewId = 111;
            const int numberOfArtifacts = 15;
            
            // Execute: 
            GetReviewArtifactsResultSet reviewArtifacts = null;
            Assert.DoesNotThrow(() => reviewArtifacts = Helper.ArtifactStore.GetReviewArtifacts(_user, reviewId),
                "Get Baseline shouldn't return an error.");

            // Verify:
            Assert.AreEqual(1, reviewArtifacts.Total);
        }
    }
}