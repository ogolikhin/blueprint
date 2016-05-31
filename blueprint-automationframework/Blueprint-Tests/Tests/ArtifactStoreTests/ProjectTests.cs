using Helper;
using Model;
using NUnit.Framework;
using CustomAttributes;
using System.Net;
using System.Collections.Generic;
using TestCommon;
using Model.ArtifactModel;
using Model.Factories;
using Model.ArtifactModel.Impl;
using Utilities;

namespace ArtifactStoreTests 
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ProjectTests : TestBase
    {
        private const int defaultProjectId = 1;
        private const int notExistingId = int.MaxValue;
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
        [TestRail(125497)]
        [Description("Executes Get project children call and returns 200 OK if successful")]
        public void GetProjectChildrenByProjectId_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                List<HttpStatusCode> expectedCodesList = new List<HttpStatusCode>();
                expectedCodesList.Add(HttpStatusCode.OK);
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetProjectChildrenByProjectId(defaultProjectId, _user, expectedCodesList);
            }, "The GET /projects/{projectId}/children endpoint should return 200 OK!");
        }

        [Test]
        [TestRail(125500)]
        [Description("Executes Get project children call and returns 404 Not Found if successful")]
        public void GetProjectChildrenByProjectId_NotFound()
        {
            Assert.Throws<Http404NotFoundException>(() =>
            {
                /*Executes get project children REST call and verify HTTP code*/
                /*NON EXISTING PROJECT (id = 99) IS USED */
                Helper.ArtifactStore.GetProjectChildrenByProjectId(notExistingId, _user);
            }, "The GET /projects/{projectId}/children endpoint should return 404 Not found!");
        }

        [Test]
        [TestRail(125501)]
        [Description("Executes Get project children call and returns 401 Unauthorized if successful")]
        public void GetProjectChildrenByProjectId_Unauthorized()
        {
            //Replace session token with expired session one
            _user.SetToken("CD4351BF-0162-4AB9-BA80-1A932D94CF7F");

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetProjectChildrenByProjectId(defaultProjectId, _user);
            }, "The GET /projects/{projectId}/children endpoint should return 401 Unauthorized!");
        }


        [Test]
        [TestRail(125502)]
        [Description("Executes Get project children call and returns 'Bad Request' if successful")]
        public void GetProjectChildrenByProjectId_BadRequest()
        {
            Assert.Throws<Http400BadRequestException>(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetProjectChildrenByProjectId(defaultProjectId, noTokenInRequest);
            }, "The GET /projects/{projectId}/children endpoint should return 400 Bad Request!");
        }


        [Test]
        [TestRail(125511)]
        [Description("Executes Get published artifact children call for published artifact and returns 200 OK if successful")]
        public void GetPublishedArtifactChildrenByProjectAndArtifactId_OK()
        {
            IProject project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);
            IOpenApiArtifact parentArtifact = CreateParentAndTwoChildrenArtifactsAndGetParentArtifact(project);

            Assert.DoesNotThrow(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(defaultProjectId, parentArtifact.Id, _user);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 200 OK!");
        }

        [Test]
        [TestRail(111)]
        [Description("Executes Get artifact children call for artifact that does not exists and returns 404 Not Found if successful")]
        public void GetPublishedArtifactChildrenByProjectAndArtifactId_NotFound()
        {
            Assert.Throws<Http404NotFoundException>(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*NON EXISTING PROJECT (id = 99) IS USED */
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(defaultProjectId, notExistingId, _user);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 404 Not found!");
        }

        [Test]
        [TestRail(111)]
        [Description("Executes Get published artifact children call and returns 401 Unauthorized if successful")]
        public void GetPublishedArtifactChildrenByProjectAndArtifactId_Unauthorized()
        {
            //Replace session token with expired session one
            _user.SetToken("CD4351BF-0162-4AB9-BA80-1A932D94CF7F");

            IProject project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);
            IOpenApiArtifact parentArtifact = CreateParentAndTwoChildrenArtifactsAndGetParentArtifact(project);

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(defaultProjectId, parentArtifact.Id, _user);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 401 Unauthorized!");
        }

        [Test]
        [TestRail(111)]
        [Description("Executes Get published artifact children call and returns 'Bad Request' if successful")]
        public void GetPublishedArtifactChildrenByProjectAndArtifactId_BadRequest()
        {
            IProject project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);
            IOpenApiArtifact parentArtifact = CreateParentAndTwoChildrenArtifactsAndGetParentArtifact(project);

            Assert.Throws<Http400BadRequestException>(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(defaultProjectId, parentArtifact.Id, noTokenInRequest);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 400 Bad Request!");
        }

        [Test]
        [TestRail(111)]
        [Description("Executes Get draft artifact children call for published artifact and returns 200 OK if successful")]
        public void GetDraftArtifactChildrenByProjectAndArtifactId_OK()
        {
            IProject project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);
            IOpenApiArtifact parentArtifact = CreateParentAndTwoChildrenArtifactsAndGetParentArtifact(project);

            //Create Description property
            IOpenApiProperty property = new OpenApiProperty(parentArtifact.Address);
            //Set property value
            parentArtifact.Properties.Add(property.SetPropertyAttribute(project, _user, BaseArtifactType.Actor, "Description", propertyValue: "Testing Set Property Value"));

            Assert.DoesNotThrow(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(defaultProjectId, parentArtifact.Id, _user);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 200 OK!");
        }

        [Test]
        [TestRail(111)]
        [Description("Executes Get draft artifact children call and returns 401 Unauthorized if successful")]
        public void GetDraftArtifactChildrenByProjectAndArtifactId_Unauthorized()
        {
            //Replace session token with expired session one
            _user.SetToken("CD4351BF-0162-4AB9-BA80-1A932D94CF7F");

            IProject project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);
            IOpenApiArtifact parentArtifact = CreateParentAndTwoChildrenArtifactsAndGetParentArtifact(project);

            //Create Description property
            IOpenApiProperty property = new OpenApiProperty(parentArtifact.Address);
            //Set property value
            parentArtifact.Properties.Add(property.SetPropertyAttribute(project, _user, BaseArtifactType.Actor, "Description", propertyValue: "Testing Set Property Value"));

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(defaultProjectId, parentArtifact.Id, _user);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 401 Unauthorized!");
        }

        [Test]
        [TestRail(111)]
        [Description("Executes Get draft artifact children call and returns 'Bad Request' if successful")]
        public void GetDraftArtifactChildrenByProjectAndArtifactId_BadRequest()
        {
            IProject project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);
            IOpenApiArtifact parentArtifact = CreateParentAndTwoChildrenArtifactsAndGetParentArtifact(project);

            //Create Description property
            IOpenApiProperty property = new OpenApiProperty(parentArtifact.Address);
            //Set property value
            parentArtifact.Properties.Add(property.SetPropertyAttribute(project, _user, BaseArtifactType.Actor, "Description", propertyValue: "Testing Set Property Value"));

            Assert.Throws<Http400BadRequestException>(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(defaultProjectId, parentArtifact.Id, noTokenInRequest);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 400 Bad Request!");
        }

        [Test]
        [TestRail(111)]
        [Description("Executes Get publish artifact of second level children call for published artifact and returns 200 OK if successful")]
        public void GetSecondLevelPublishedArtifactChildrenByProjectAndArtifactId_OK()
        {
            IProject project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);

            List<IOpenApiArtifact> parentArtifactList = CreateGrandParentAndTwoParentArtifactsAndChildrOfSecondParentAndGetSecondParentArtifact(project);

            Assert.DoesNotThrow(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(defaultProjectId, parentArtifactList[1].Id, _user);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 200 OK!");
        }

        [Test]
        [TestRail(111)]
        [Description("Executes Get publish artifact of second level children call and returns 401 Unauthorized if successful")]
        public void GetSecondLevelPublishedArtifactChildrenByProjectAndArtifactId_Unauthorized()
        {
            //Replace session token with expired session one
            _user.SetToken("CD4351BF-0162-4AB9-BA80-1A932D94CF7F");

            IProject project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);
            List<IOpenApiArtifact> parentArtifactList = CreateGrandParentAndTwoParentArtifactsAndChildrOfSecondParentAndGetSecondParentArtifact(project);

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(defaultProjectId, parentArtifactList[1].Id, _user);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 401 Unauthorized!");
        }

        [Test]
        [TestRail(111)]
        [Description("Executes Get publish artifact of second level children call and returns 'Bad Request' if successful")]
        public void GetSecondLevelPublishedArtifactChildrenByProjectAndArtifactId_BadRequest()
        {
            IProject project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);
            List<IOpenApiArtifact> parentArtifactList = CreateGrandParentAndTwoParentArtifactsAndChildrOfSecondParentAndGetSecondParentArtifact(project);

            Assert.Throws<Http400BadRequestException>(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(defaultProjectId, parentArtifactList[1].Id, noTokenInRequest);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 400 Bad Request!");
        }

        [Test]
        [TestRail(111)]
        [Description("Executes Get draft artifact of second level children call for published artifact and returns 200 OK if successful")]
        public void GetSecondLevelDraftArtifactChildrenByProjectAndArtifactId_OK()
        {
            IProject project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);
            List<IOpenApiArtifact> parentArtifactList = CreateGrandParentAndTwoParentArtifactsAndChildrOfSecondParentAndGetSecondParentArtifact(project);

            //Create Description property
            IOpenApiProperty property = new OpenApiProperty(parentArtifactList[1].Address);
            //Set property value
            parentArtifactList[1].Properties.Add(property.SetPropertyAttribute(project, _user, BaseArtifactType.Actor, "Description", propertyValue: "Testing Set Property Value"));

            Assert.DoesNotThrow(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(defaultProjectId, parentArtifactList[1].Id, _user);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 200 OK!");
        }

        [Test]
        [TestRail(111)]
        [Description("Executes Get draft artifact of second level children call and returns 401 Unauthorized if successful")]
        public void GetSecondLevelDraftArtifactChildrenByProjectAndArtifactId_Unauthorized()
        {
            //Replace session token with expired session one
            _user.SetToken("CD4351BF-0162-4AB9-BA80-1A932D94CF7F");

            IProject project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);
            List<IOpenApiArtifact> parentArtifactList = CreateGrandParentAndTwoParentArtifactsAndChildrOfSecondParentAndGetSecondParentArtifact(project);

            //Create Description property
            IOpenApiProperty property = new OpenApiProperty(parentArtifactList[1].Address);
            //Set property value
            parentArtifactList[1].Properties.Add(property.SetPropertyAttribute(project, _user, BaseArtifactType.Actor, "Description", propertyValue: "Testing Set Property Value"));

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(defaultProjectId, parentArtifactList[1].Id, _user);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 401 Unauthorized!");
        }

        [Test]
        [TestRail(111)]
        [Description("Executes Get draft artifact of second level children call and returns 'Bad Request' if successful")]
        public void GetSecondLevelDraftArtifactChildrenByProjectAndArtifactId_BadRequest()
        {
            IProject project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);
            List<IOpenApiArtifact> parentArtifactList = CreateGrandParentAndTwoParentArtifactsAndChildrOfSecondParentAndGetSecondParentArtifact(project);

            //Create Description property
            IOpenApiProperty property = new OpenApiProperty(parentArtifactList[1].Address);
            //Set property value
            parentArtifactList[1].Properties.Add(property.SetPropertyAttribute(project, _user, BaseArtifactType.Actor, "Description", propertyValue: "Testing Set Property Value"));

            Assert.Throws<Http400BadRequestException>(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(defaultProjectId, parentArtifactList[1].Id, noTokenInRequest);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 400 Bad Request!");
        }

        [Test]
        [TestRail(111)]
        [Description("Executes Get publish artifact of second level children call for published artifact, creates orphan artifact and returns 200 OK if successful")]
        public void GetChildrenOfMovedArtifactId_OK()
        {
            IProject project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);
            List<IOpenApiArtifact> parentArtifactList = CreateGrandParentAndTwoParentArtifactsAndChildrOfSecondParentAndGetSecondParentArtifact(project);

            parentArtifactList[1].ParentId = parentArtifactList[0].Id;
            parentArtifactList[1].Save();
            parentArtifactList[0].Publish();

            Assert.DoesNotThrow(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(defaultProjectId, parentArtifactList[1].Id, _user);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 200 OK!");
        }

        [Test]
        [Description("Executes Get publish artifact of second level children call and returns 401 Unauthorized if successful")]
        public void GetChildrenOfMovedArtifactId_Unauthorized()
        {
            //Replace session token with expired session one
            _user.SetToken("CD4351BF-0162-4AB9-BA80-1A932D94CF7F");

            IProject project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);
            List<IOpenApiArtifact> parentArtifactList = CreateGrandParentAndTwoParentArtifactsAndChildrOfSecondParentAndGetSecondParentArtifact(project);

            parentArtifactList[1].ParentId = parentArtifactList[0].Id;
            parentArtifactList[1].Save();
            parentArtifactList[0].Publish();

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(defaultProjectId, parentArtifactList[1].Id, _user);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 401 Unauthorized!");
        }

        [Test]
        [Description("Executes Get publish artifact of second level children call and returns 'Bad Request' if successful")]
        public void GetChildrenOfMovedArtifactId_BadRequest()
        {
            IProject project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);
            List<IOpenApiArtifact> parentArtifactList = CreateGrandParentAndTwoParentArtifactsAndChildrOfSecondParentAndGetSecondParentArtifact(project);

            parentArtifactList[1].ParentId = parentArtifactList[0].Id;
            parentArtifactList[1].Save();
            parentArtifactList[0].Publish();

            Assert.Throws<Http400BadRequestException>(() =>
            {
                /*Executes get project children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(defaultProjectId, parentArtifactList[1].Id, noTokenInRequest);
            }, "The GET /projects/{projectId}/artifacts/{artifactId}children endpoint should return 400 Bad Request!");
        }

        private IOpenApiArtifact CreateParentAndTwoChildrenArtifactsAndGetParentArtifact(IProject project)
        {
            IOpenApiArtifact parentArtifact, childArtifact;

            //Create parent artifact with ArtifactType and populate all required values without properties
            parentArtifact = Helper.CreateOpenApiArtifact(project, _user, artifactType: BaseArtifactType.Document);
            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            parentArtifact.Save();

            //Create first child artifact with ArtifactType and populate all required values without properties
            childArtifact = Helper.CreateOpenApiArtifact(project, _user, artifactType: BaseArtifactType.Document);
            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            childArtifact.ParentId = parentArtifact.Id;
            childArtifact.Save();

            //Create second child artifact with ArtifactType and populate all required values without properties
            childArtifact = Helper.CreateOpenApiArtifact(project, _user, artifactType: BaseArtifactType.Document);
            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            childArtifact.ParentId = parentArtifact.Id;
            childArtifact.Save();
            //Publish artifact
            childArtifact.Publish();

            return parentArtifact;
        }

        private List<IOpenApiArtifact> CreateGrandParentAndTwoParentArtifactsAndChildrOfSecondParentAndGetSecondParentArtifact(IProject project)
        {
            IOpenApiArtifact grandParentArtifact, parentArtifact, childArtifact;

            List<IOpenApiArtifact> parentArtifactList = new List<IOpenApiArtifact>();

            //Create grand parent artifact with ArtifactType and populate all required values without properties
            grandParentArtifact = Helper.CreateOpenApiArtifact(project, _user, artifactType: BaseArtifactType.Document);
            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            grandParentArtifact.Save();

            //Create first parent artifact with ArtifactType and populate all required values without properties
            parentArtifact = Helper.CreateOpenApiArtifact(project, _user, artifactType: BaseArtifactType.Document);
            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            parentArtifact.ParentId = grandParentArtifact.Id;
            parentArtifact.Save();
            parentArtifactList.Add(parentArtifact);

            //Create second parent artifact with ArtifactType and populate all required values without properties
            parentArtifact = Helper.CreateOpenApiArtifact(project, _user, artifactType: BaseArtifactType.Document);
            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            parentArtifact.ParentId = grandParentArtifact.Id;
            parentArtifact.Save();
            parentArtifactList.Add(parentArtifact);

            //Create child artifact of second parent with ArtifactType and populate all required values without properties
            childArtifact = Helper.CreateOpenApiArtifact(project, _user, artifactType: BaseArtifactType.Document);
            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            childArtifact.ParentId = parentArtifact.Id;
            childArtifact.Save();

            //Publish artifact
            childArtifact.Publish();

            return parentArtifactList;
        }
    }
}
