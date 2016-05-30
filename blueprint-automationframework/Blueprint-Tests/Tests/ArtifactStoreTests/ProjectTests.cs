using Helper;
using Model;
using NUnit.Framework;
using CustomAttributes;
using System.Net;
using System.Collections.Generic;
using TestCommon;

namespace ArtifactStoreTests 
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ProjectTests : TestBase
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
        [TestRail(125497)]
        [Description("Executes Get project children call and returns 200 OK if successful")]
        public void GetProjectChildrenByProjectId_OK()
        {
            using (TestHelper helper = new TestHelper())
            {
                List<HttpStatusCode> expectedCodesList = new List<HttpStatusCode>();

                Assert.DoesNotThrow(() =>
                {
                    expectedCodesList.Add(HttpStatusCode.OK);
                    /*Executes get project children REST call and returns HTTP code*/
                    /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                    helper.ArtifactStore.GetProjectChildrenByProjectId(defaultProjectId, _user, expectedCodesList);
                }, "The GET /projects/{projectId}/children endpoint should return 200 OK!");
            }
        }

        [Test]
        [TestRail(125500)]
        [Description("Executes Get project children call and returns 404 Not Found if successful")]
        public void GetProjectChildrenByProjectId_NotFound()
        {
            using (TestHelper helper = new TestHelper())
            {
                List<HttpStatusCode> expectedCodesList = new List<HttpStatusCode>();

                Assert.DoesNotThrow(() =>
                {
                    expectedCodesList.Add(HttpStatusCode.NotFound);
                    /*Executes get project children REST call and returns HTTP code*/
                    /*NON EXISTING PROJECT (id = 99) IS USED */
                    helper.ArtifactStore.GetProjectChildrenByProjectId(nonExistingProject, _user, expectedCodesList);
                }, "The GET /projects/{projectId}/children endpoint should return 404 Not found!");
            }
        }

        [Test]
        [TestRail(125501)]
        [Description("Executes Get project children call and returns 401 Unauthorized if successful")]
        public void GetProjectChildrenByProjectId_Unauthorized()
        {
            using (TestHelper helper = new TestHelper())
            {
                //Replace session token with expired session one
                _user.SetToken("CD4351BF-0162-4AB9-BA80-1A932D94CF7F");

                List<HttpStatusCode> expectedCodesList = new List<HttpStatusCode>();

                Assert.DoesNotThrow(() =>
                {
                    expectedCodesList.Add(HttpStatusCode.Unauthorized);
                    /*Executes get project children REST call and returns HTTP code*/
                    /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                    helper.AdminStore.GetProjectById(defaultProjectId, _user, expectedCodesList);
                }, "The GET /projects/{projectId}/children endpoint should return 401 Unauthorized!");
            }
        }


        [Test]
        [TestRail(125502)]
        [Description("Executes Get project children call and returns 'Bad Request' if successful")]
        public static void GetProjectChildrenByProjectId_BadRequest()
        {
            List<HttpStatusCode> expectedCodesList = new List<HttpStatusCode>();
            expectedCodesList.Add(HttpStatusCode.BadRequest);

            /*Executes get project children REST call and returns HTTP code*/
            /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
            using (TestHelper helper = new TestHelper())
            {
                Assert.DoesNotThrow(() =>
                {
                    helper.AdminStore.GetProjectById(defaultProjectId, null, expectedCodesList);
                }, "The GET /projects/{projectId}/children endpoint should return 400 Bad Request!");
            }
        }

    }
}
