using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using NUnit.Framework;
using TestCommon;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    class ArtifactDetailsTests : TestBase
    {
        private IUser _user = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region GetProjectChildrenByProjectId

        [TestCase]
        [TestRail(154601)]
        [Description("Executes artifact details call for published artifact and returns 200 OK if successful")]
        public void GetArtifactDetailsByArtifactId_OK()
        {
            IOpenApiArtifact artifact;

            IProject project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);

            //Create parent artifact with ArtifactType and populate all required values without properties
            artifact = Helper.CreateOpenApiArtifact(project, _user, artifactType: BaseArtifactType.Process);
            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            artifact.Save();
            artifact.Publish();

            Assert.DoesNotThrow(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetArtifactDetailsByArtifactId(artifact.Id, _user);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 200 OK!");
        }
    }
}