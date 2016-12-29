using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
using Model.JobModel.Impl;
using NUnit.Framework;
using System.Linq;
using TestCommon;

namespace CommonServiceTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class JobsTests : TestBase
    {
        private IProject _project = null;
        private IUser _adminUser = null;
        //private IUser _authorUser = null;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);
            //_authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        // TODO: Create the dedicated baseline artifact for creating ALM ChangeSummary Job
        [Category(Categories.CustomData)]
        [TestCase(157)]
        [TestRail(0)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Create an ALM job. Verify that the job information gets retrieved with GET Jobs.")]
        public void GetJobs_AddJob_VerifyGetJobs(int baselineArtifactId)
        {
            // Setup:
            IProject projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            // Get the ALM Target that will be used for a sample job
            var almTarget = AlmTarget.GetAlmTargets(Helper.ArtifactStore.Address, _adminUser, projectCustomData).First();
            // Create a change summary job
            OpenAPIJob.AddAlmChangeSummaryJob(Helper.ArtifactStore.Address, _adminUser, projectCustomData, baselineArtifactId, almTarget.Id);

            // Execute:
            Helper.AdminStore.GetJobs(_adminUser, page: null, pageSize: null, jobType: null/*Model.JobModel.Enums.JobType.HpAlmRestChangeSummary*/);
            // Verify:
        }

        #endregion 200 OK Tests
    }
}
