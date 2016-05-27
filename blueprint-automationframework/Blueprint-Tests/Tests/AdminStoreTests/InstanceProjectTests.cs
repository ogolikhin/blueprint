using System.Collections.Generic;
using NUnit.Framework;
using CustomAttributes;
using Model.Factories;
using Helper;
using System.Net;
using Model;
using TestCommon;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class InstanceProjectTests : TestBase
    {
        private const int defaultProjectId = 1;
        private const int nonExistingProject = 99;

        private IUser _user = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [Test]
        [TestRail(123258)]
        [Description("Gets the project and returns 'OK' if successful")]
        public void GetProjectById_OK()
        {
            // Get a valid Access Control token for the user (for the new REST calls).
            using (TestHelper helper = new TestHelper())
            {
                List<HttpStatusCode> expectedCodesList = new List<HttpStatusCode>();
                expectedCodesList.Add(HttpStatusCode.OK);

                /*Executes get project REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) USED */
                helper.AdminStore.GetProjectById(defaultProjectId, _user, expectedCodesList);
            }
        }

        [Test]
        [TestRail(123269)]
        [Description("Gets the project and returns 'Not Found' if successfull")]
        public void GetProjectById_NotFound()
        {
            // Get a valid Access Control token for the user (for the new REST calls).
            using (TestHelper helper = new TestHelper())
            {
                List<HttpStatusCode> expectedCodesList = new List<HttpStatusCode>();
                expectedCodesList.Add(HttpStatusCode.NotFound);

                /*Executes get project REST call and returns HTTP code*/
                helper.AdminStore.GetProjectById(nonExistingProject, _user, expectedCodesList);
            }
        }

        [Test]
        [TestRail(123271)]
        [Description("Gets the project and returns 'Unauthorized' if successful")]
        public void GetProjectById_Unauthorized()
        {
            // Get a valid Access Control token for the user (for the new REST calls).
            using (TestHelper helper = new TestHelper())
            {
                _user.SetToken("CD4351BF-0162-4AB9-BA80-1A932D94CF7F");

                List<HttpStatusCode> expectedCodesList = new List<HttpStatusCode>();
                expectedCodesList.Add(HttpStatusCode.Unauthorized);

                /*Executes get project REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                helper.AdminStore.GetProjectById(defaultProjectId, _user, expectedCodesList);
            }
        }

        [Test]
        [TestRail(123272)]
        [Description("Executes Get project call and returns 'Bad Request' if successful")]
        public static void GetProjectById_BadRequest()
        {
            List<HttpStatusCode> expectedCodesList = new List<HttpStatusCode>();
            expectedCodesList.Add(HttpStatusCode.BadRequest);

            /*Executes get project REST call and returns HTTP code*/
            /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
            using (TestHelper helper = new TestHelper())
            {
                helper.AdminStore.GetProjectById(defaultProjectId, null, expectedCodesList);
            }
        }
    }
}
