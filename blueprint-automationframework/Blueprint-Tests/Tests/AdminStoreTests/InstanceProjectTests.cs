using System.Collections.Generic;
using NUnit.Framework;
using CustomAttributes;
using Helper;
using System.Net;
using Model;
using TestCommon;
using Utilities;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class InstanceProjectTests : TestBase
    {
        private const int defaultProjectId = 1;
        private const int nonExistingProject = int.MaxValue;
        private const IUser noTokenInRequest = null;

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
            /*Executes get project REST call and returns HTTP code*/
            /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) USED */
            Helper.AdminStore.GetProjectById(defaultProjectId, _user);
        }

        [Test]
        [TestRail(123269)]
        [Description("Gets the project and returns 'Not Found' if successfull")]
        public void GetProjectById_NotFound()
        {
            Assert.Throws<Http404NotFoundException>(() =>
            {
                /*Executes get project REST call and returns HTTP code*/
                Helper.AdminStore.GetProjectById(nonExistingProject, _user);
            }, "AdminStore should return a 404 Not Found error when trying to call non existing project");
        }

        [Test]
        [TestRail(123271)]
        [Description("Gets the project and returns 'Unauthorized' if successful")]
        public void GetProjectById_Unauthorized()
        {
            // Get a valid Access Control token for the user (for the new REST calls).
            _user.SetToken("CD4351BF-0162-4AB9-BA80-1A932D94CF7F");

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                /*Executes get project REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.AdminStore.GetProjectById(defaultProjectId, _user);
            }, "AdminStore should return a 401 Unauthorized error when trying to call with expired token");
        }

        [Test]
        [TestRail(123272)]
        [Description("Executes Get project call and returns 'Bad Request' if successful")]
        public void GetProjectById_BadRequest()
        {
            Assert.Throws<Http400BadRequestException>(() =>
            {
                /*Executes get project REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.AdminStore.GetProjectById(defaultProjectId, noTokenInRequest);
            }, "AdminStore should return a 400 Bad reques error when trying to call without session token");
        }
    }
}
