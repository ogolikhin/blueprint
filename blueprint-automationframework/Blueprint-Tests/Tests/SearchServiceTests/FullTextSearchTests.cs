using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using System;
using TestCommon;

namespace SearchServiceTests
{
    [TestFixture]
    [Category(Categories.SearchService)]
    public static class FullTextSearchTests //: TestBase
    {
        const string FULLTEXTSEARCH_PATH = RestPaths.Svc.SearchService.FULLTEXTSEARCH;

        //private IUser _user = null;
        //private IProject _project = null;

        [SetUp]
        public static void SetUp()
        {
            throw new NotImplementedException();
            //Helper = new TestHelper();
            //_user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            //_project = ProjectFactory.GetProject(_user);
        }

        [TearDown]
        public static void TearDown()
        {
            throw new NotImplementedException();
            //Helper?.Dispose();
        }

        #region 200 OK Tests
        #endregion 200 OK Tests

        #region 400 Bad Request Tests
        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests
        #endregion 401 Unauthorized Tests

        #region 404 Not Found Tests
        #endregion 404 Not Found Tests

        #region 409 Conflict Tests
        #endregion 409 Conflict Tests

        #region Private Functions
        #endregion Private Functions

    }
}
