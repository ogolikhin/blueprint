using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class LegacyArtifactHistoryTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestRail(182934)]
        [Description("Create & publish a diagram artifact, Get DiagramArtifact.  Verify that the latest version of valid diagram artifact is returned.")]
        public void GetDiagramArtifact_PublishAndGetDiagamArtifact_ReturnsLatestVersionOfDiagramArtifact(BaseArtifactType artifactType)
        {
            // Setup: Create and publish a diagram artifact
            var publishedDiagramArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType: artifactType);
            var retrievedArtifact = OpenApiArtifact.GetArtifact(publishedDiagramArtifact.Address, _project, publishedDiagramArtifact.Id, _user);
            NovaDiagramArtifact diagramArtifact = null;

            // Execute: Get the diagram artifact using GetDiagramArtifact
            Assert.DoesNotThrow(() => {
                diagramArtifact = Helper.ArtifactStore.GetDiagramArtifact(_user, publishedDiagramArtifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.DIAGRAM_id_);

            // Validation: Verify that the returned from GetDiagramArtifact is in valid format
            ArtifactStoreHelper.AssertArtifactsEqual(diagramArtifact, retrievedArtifact);
        }

        [TestCase]
        [TestRail(183019)]
        [Description("Create & publish a glossary artifact, Get GlossaryArtifact.  Verify that the latest version of valid glossary artifact is returned.")]
        public void GetGlossaryArtifact_PublishAndGetGlossaryArtifact_ReturnsLatestVersionOfGlossaryArtifact()
        {
            // Setup: Create and publish a glossary artifact
            var publishedGlossaryArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType: BaseArtifactType.Glossary);
            var retrievedArtifact = OpenApiArtifact.GetArtifact(publishedGlossaryArtifact.Address, _project, publishedGlossaryArtifact.Id, _user);
            NovaGlossaryArtifact glossaryArtifact = null;

            // Execute: Get the glossary artifact using GetGlossaryArtifact
            Assert.DoesNotThrow(() => {
                glossaryArtifact = Helper.ArtifactStore.GetGlossaryArtifact(_user, publishedGlossaryArtifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.GLOSSARY_id_);

            // Validation: Verify that the returned from GetGlossaryArtifact is in valid format
            ArtifactStoreHelper.AssertArtifactsEqual(glossaryArtifact, retrievedArtifact);
        }

        [TestCase]
        [TestRail(183020)]
        [Description("Create & publish a use case artifact, Get UseCaseArtifact.  Verify that the latest version of valid use case artifact is returned.")]
        public void GetUseCaseArtifact_PublishAndGetUseCaseArtifact_ReturnsLatestVersionOfUseCaseArtifact()
        {
            // Setup: Create and publish a use case artifact
            var publishedUseCaseArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);
            var retrievedArtifact = OpenApiArtifact.GetArtifact(publishedUseCaseArtifact.Address, _project, publishedUseCaseArtifact.Id, _user);
            NovaUseCaseArtifact usecaseArtifact = null;

            // Execute: Get the use case artifact using GetUseCaseArtifact
            Assert.DoesNotThrow(() => {
                usecaseArtifact = Helper.ArtifactStore.GetUseCaseArtifact(_user, publishedUseCaseArtifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.USECASE_id_);

            // Validation: Verify that the returned from GetUseCaseArtifact is in valid format
            ArtifactStoreHelper.AssertArtifactsEqual(usecaseArtifact, retrievedArtifact);
        }

        [TestCase(2,BaseArtifactType.DomainDiagram)]
        [TestCase(3,BaseArtifactType.GenericDiagram)]
        [TestCase(4,BaseArtifactType.UseCaseDiagram)]
        [TestRail(183352)]
        [Description("Create & publish a diagram artifact multiple times to have multiple version of it, Get diagram artifact without version. Verify that latest version of artifact is returned.")]
        public void GetDiagramArtifact_PublishAndGetDiagramArtifactWithoutSpecificVersion_ReturnsLatestVersionOfDiagramArtifact(int numberOfVersions, BaseArtifactType artifactType)
        {
            // Setup: Create and publish a diagram artifact multiple times to have multiple versions of it
            var publishedDiagramArtifact = Helper.CreateAndPublishOpenApiArtifact(_project, _user, artifactType: artifactType, numberOfVersions: numberOfVersions);
            // getting the latest version of the artifact using open API GetArtifact
            var retrievedArtifact = OpenApiArtifact.GetArtifact(publishedDiagramArtifact.Address, _project, publishedDiagramArtifact.Id, _user);
            NovaDiagramArtifact diagramArtifact = null;

            // Execute: Get the diagram artifact using GetDiagramArtifact without versionId parameter
            Assert.DoesNotThrow(() => {
                diagramArtifact = Helper.ArtifactStore.GetDiagramArtifact(_user, publishedDiagramArtifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.DIAGRAM_id_);

            // Validation: Verify that the returned from GetDiagramArtifact in valid format
            ArtifactStoreHelper.AssertArtifactsEqual(diagramArtifact, retrievedArtifact);
        }

        [TestCase(4)]
        [TestRail(183353)]
        [Description("Create & publish a glossary artifact multiple times to have multiple version of it, Get glossary artifact without version. Verify that latest version of artifact is returned.")]
        public void GetGlossaryArtifact_PublishAndGetGlossaryArtifactWithoutSpecificVersion_ReturnsLatestVersionOfGlossaryArtifact(int numberOfVersions)
        {
            // Setup: Create and publish a glossary artifact multiple times to have multiple versions of it
            var publishedGlossaryArtifact = Helper.CreateAndPublishOpenApiArtifact(_project, _user, BaseArtifactType.Glossary, numberOfVersions: numberOfVersions);
            // getting the latest version of the artifact using open API GetArtifact
            var retrievedArtifact = OpenApiArtifact.GetArtifact(publishedGlossaryArtifact.Address, _project, publishedGlossaryArtifact.Id, _user);
            NovaGlossaryArtifact diagramArtifact = null;

            // Execute: Get the glossary artifact using GetGlossaryArtifact without versionId parameter
            Assert.DoesNotThrow(() => {
                diagramArtifact = Helper.ArtifactStore.GetGlossaryArtifact(_user, publishedGlossaryArtifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.GLOSSARY_id_);

            // Validation: Verify that the returned from GetDiagramArtifact in valid format
            ArtifactStoreHelper.AssertArtifactsEqual(diagramArtifact, retrievedArtifact);
        }

        [TestCase(4)]
        [TestRail(183354)]
        [Description("Create & publish a use case artifact multiple times to have multiple version of it, Get use case artifact without passing version. Verify that latest version of artifact is returned.")]
        public void GetUseCaseArtifact_PublishAndGetUseCaseArtifactWithoutSpecificVersion_ReturnsLatestVersionOfUseCaseArtifact(int numberOfVersions)
        {
            // Setup: Create and publish a use case artifact multiple times to have multiple versions of it
            var publishedUseCaseArtifact = Helper.CreateAndPublishOpenApiArtifact(_project, _user, BaseArtifactType.UseCase, numberOfVersions: numberOfVersions);
            // getting the latest version of the artifact using open API GetArtifact
            var retrievedArtifact = OpenApiArtifact.GetArtifact(publishedUseCaseArtifact.Address, _project, publishedUseCaseArtifact.Id, _user);
            NovaUseCaseArtifact usecaseArtifact = null;

            // Execute: Get the use case artifact using GetUseCaseArtifact without passing versionId parameter
            Assert.DoesNotThrow(() => {
                usecaseArtifact = Helper.ArtifactStore.GetUseCaseArtifact(_user, publishedUseCaseArtifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.USECASE_id_);

            // Validation: Verify that the returned from GetUseCaseArtifact in valid format
            ArtifactStoreHelper.AssertArtifactsEqual(usecaseArtifact, retrievedArtifact);
        }

        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestRail(183355)]
        [Description("Create & publish a diagram artifact, modify & publish it again, GetDiagramArtifact with versionId=1. Verify that first version of diagram artifact is returned.")]
        public void GetDiagramArtifact_PublishAndGetDiagramArtifactWithVersion1_ReturnsFirstVersionOfDiagramArtifact(BaseArtifactType artifactType)
        {
            // Setup: Create and publish a diagram artifact two times to have two versions of it			
            IArtifact publishedDiagramArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType: artifactType);
            var retrievedArtifactVersion1 = OpenApiArtifact.GetArtifact(publishedDiagramArtifact.Address, _project, publishedDiagramArtifact.Id, _user);

            // These are internal properties used by automation, so OpenAPI doesn't set them for us.
            retrievedArtifactVersion1.Address = publishedDiagramArtifact.Address;
            retrievedArtifactVersion1.CreatedBy = publishedDiagramArtifact.CreatedBy;

            // Modify & publish the artifact.
            var retrievedArtifactVersion2 = retrievedArtifactVersion1.DeepCopy();
            retrievedArtifactVersion2.Name = I18NHelper.FormatInvariant("{0}-version2", retrievedArtifactVersion1.Name);

            Artifact.SaveArtifact(retrievedArtifactVersion2, _user);
            retrievedArtifactVersion2.Publish();

            NovaDiagramArtifact diagramArtifact = null;

            // Execute: Get the diagram artifact using GetDiagramArtifact with first versionId			
            Assert.DoesNotThrow(() => {
                diagramArtifact = Helper.ArtifactStore.GetDiagramArtifact(_user, publishedDiagramArtifact.Id, versionId: 1);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.DIAGRAM_id_);

            ArtifactStoreHelper.AssertArtifactsEqual(diagramArtifact, retrievedArtifactVersion1);

            Assert.IsEmpty(diagramArtifact.SpecificPropertyValues, "SpecificPropertyValues isn't implemented yet so it should be empty!");
        }

        [TestCase]
        [TestRail(183356)]
        [Description("Create & publish a glossary artifact, modify & publish it again, GetGlossaryArtifact with versionId=1. Verify that first version of glossary artifact is returned.")]
        public void GetGlossaryArtifact_PublishAndGetGlossaryArtifactWithVersion1_ReturnsFirstVersionOfGlossaryArtifact()
        {
            // Setup: Create and publish a glossary artifact two times to have two versions of it			
            IArtifact publishedGlossaryArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Glossary);
            var retrievedArtifactVersion1 = OpenApiArtifact.GetArtifact(publishedGlossaryArtifact.Address, _project, publishedGlossaryArtifact.Id, _user);

            // These are internal properties used by automation, so OpenAPI doesn't set them for us.
            retrievedArtifactVersion1.Address = publishedGlossaryArtifact.Address;
            retrievedArtifactVersion1.CreatedBy = publishedGlossaryArtifact.CreatedBy;

            // Modify & publish the artifact.
            var retrievedArtifactVersion2 = retrievedArtifactVersion1.DeepCopy();
            retrievedArtifactVersion2.Name = I18NHelper.FormatInvariant("{0}-version2", retrievedArtifactVersion1.Name);

            Artifact.SaveArtifact(retrievedArtifactVersion2, _user);
            retrievedArtifactVersion2.Publish();

            NovaGlossaryArtifact glossaryArtifact = null;

            // Execute: Get the glossary artifact using GetGlossaryArtifact with first versionId			
            Assert.DoesNotThrow(() => {
                glossaryArtifact = Helper.ArtifactStore.GetGlossaryArtifact(_user, publishedGlossaryArtifact.Id, versionId: 1);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.GLOSSARY_id_);

            ArtifactStoreHelper.AssertArtifactsEqual(glossaryArtifact, retrievedArtifactVersion1);

            Assert.IsEmpty(glossaryArtifact.SpecificPropertyValues, "SpecificPropertyValues isn't implemented yet so it should be empty!");
        }

        [TestCase]
        [TestRail(183357)]
        [Description("Create & publish a use case artifact, modify & publish it again, GetUseCaseArtifact with versionId=1. Verify that first version of use case artifact is returned.")]
        public void GetUseCaseArtifact_PublishAndGetUseCaseArtifactWithVersion1_ReturnsFirstVersionOfUseCaseArtifact()
        {
            // Setup: Create and publish a use case artifact two times to have two versions of it			
            IArtifact publishedUseCaseArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);
            var retrievedArtifactVersion1 = OpenApiArtifact.GetArtifact(publishedUseCaseArtifact.Address, _project, publishedUseCaseArtifact.Id, _user);

            // These are internal properties used by automation, so OpenAPI doesn't set them for us.
            retrievedArtifactVersion1.Address = publishedUseCaseArtifact.Address;
            retrievedArtifactVersion1.CreatedBy = publishedUseCaseArtifact.CreatedBy;

            // Modify & publish the artifact.
            var retrievedArtifactVersion2 = retrievedArtifactVersion1.DeepCopy();
            retrievedArtifactVersion2.Name = I18NHelper.FormatInvariant("{0}-version2", retrievedArtifactVersion1.Name);

            Artifact.SaveArtifact(retrievedArtifactVersion2, _user);
            retrievedArtifactVersion2.Publish();

            NovaUseCaseArtifact usecaseArtifact = null;

            // Execute: Get the use case artifact using GetUseCaseArtifact with first versionId			
            Assert.DoesNotThrow(() => {
                usecaseArtifact = Helper.ArtifactStore.GetUseCaseArtifact(_user, publishedUseCaseArtifact.Id, versionId: 1);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.USECASE_id_);

            ArtifactStoreHelper.AssertArtifactsEqual(usecaseArtifact, retrievedArtifactVersion1);

            Assert.IsEmpty(usecaseArtifact.SpecificPropertyValues, "SpecificPropertyValues isn't implemented yet so it should be empty!");
        }

        #endregion 200 OK Tests

        #region 401 Unauthorized Tests


        [TestCase("", BaseArtifactType.DomainDiagram)]
        [TestCase("invalidTokenString", BaseArtifactType.GenericDiagram)]
        [TestCase("0000000000000000-00", BaseArtifactType.UseCaseDiagram)]
        [TestRail(183033)]
        [Description("Create & publish a diagram artifact, Get DiagramArtifact with invalid token header.  Verify 401 Unauthorized.")]
        public void GetDiagramArtifact_PublishAndGetDiagamArtifactWithInvalidTokenHeader_401Unauthorized(string token, BaseArtifactType artifactType)
        {
            // Setup: Create and publish a diagram artifact
            var publishedDiagramArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType: artifactType);
            NovaDiagramArtifact diagramArtifact = null;

            IUser userWithBadOrMissingToken = UserFactory.CreateUserAndAddToDatabase();
            userWithBadOrMissingToken.Token.SetToken(_user.Token.AccessControlToken);
            userWithBadOrMissingToken.Token.AccessControlToken = token;

            // Execute: Get the diagram artifact with invalid token header using GetDiagramArtifact
            Assert.Throws<Http401UnauthorizedException>(() => diagramArtifact = Helper.ArtifactStore.GetDiagramArtifact(userWithBadOrMissingToken, publishedDiagramArtifact.Id, versionId: 1), "GetDiagramArtifact call with invalid token header does not exit with 401 UnauthorizedException!");
        }

        [TestCase("")]
        [TestCase("invalidTokenString")]
        [TestRail(183034)]
        [Description("Create & publish a glossary artifact, Get GlossaryArtifact with invalid token header.  Verify 401 Unauthorized.")]
        public void GetGlossaryArtifact_PublishAndGetGlossaryArtifactWithInvalidTokenHeader_401Unauthorized(string token)                   
        {
            // Setup: Create and publish a glossary artifact
            var publishedGlossaryArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Glossary);
            NovaGlossaryArtifact glossaryArtifact = null;

            IUser userWithBadOrMissingToken = UserFactory.CreateUserAndAddToDatabase();
            userWithBadOrMissingToken.Token.SetToken(_user.Token.AccessControlToken);
            userWithBadOrMissingToken.Token.AccessControlToken = token;

            // Execute: Get the glossary artifact with invalid token header using GetGlossaryArtifact
            Assert.Throws<Http401UnauthorizedException>(() => glossaryArtifact = Helper.ArtifactStore.GetGlossaryArtifact(userWithBadOrMissingToken, publishedGlossaryArtifact.Id, versionId: 1), "GetGlossaryArtifact call with invalid token header does not exit with 401 UnauthorizedException!");
        }

        [TestCase("")]
        [TestCase("invalidTokenString")]
        [TestRail(183035)]
        [Description("Create & publish a use case artifact, Get UseCaseArtifact with invalid token header.  Verify 401 Unauthorized.")]
        public void GetUseCaseArtifact_PublishAndGetUseCaseArtifactWithInvalidTokenHeader_401Unauthorized(string token)
        {
            // Setup: Create and publish a use case artifact
            var publishedUseCaseArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);
            NovaUseCaseArtifact usecaseArtifact = null;

            IUser userWithBadOrMissingToken = UserFactory.CreateUserAndAddToDatabase();
            userWithBadOrMissingToken.Token.SetToken(_user.Token.AccessControlToken);
            userWithBadOrMissingToken.Token.AccessControlToken = token;

            // Execute: Get the use case artifact with invalid token header using GetUseCaseArtifact
            Assert.Throws<Http401UnauthorizedException>(() => usecaseArtifact = Helper.ArtifactStore.GetUseCaseArtifact(userWithBadOrMissingToken, publishedUseCaseArtifact.Id, versionId: 1), "GetUseCaseArtifact call with invalid token header does not exit with 401 UnauthorizedException!");
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests
        #endregion 403 Forbidden Tests

        #region 404 Not Found Tests

        [TestCase(0, BaseArtifactType.DomainDiagram)]
        [TestCase(-10, BaseArtifactType.GenericDiagram)]
        [TestCase(999, BaseArtifactType.UseCaseDiagram)]
        [TestRail(183027)]
        [Description("Create & publish a diagram artifact, Get DiagramArtifact with invalid versionId.  Verify 404 NotFound.")]
        public void GetDiagramArtifact_PublishAndGetDiagamArtifactWithInvalidVersionId_404NotFound(int versionId, BaseArtifactType artifactType)
        {
            // Setup: Create and publish a diagram artifact
            var publishedDiagramArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType: artifactType);
            NovaDiagramArtifact diagramArtifact = null;

            // Execute: Get the diagram artifact with invalid versionId using GetDiagramArtifact
            var ex = Assert.Throws<Http404NotFoundException>(() => diagramArtifact = Helper.ArtifactStore.GetDiagramArtifact(_user, publishedDiagramArtifact.Id, versionId: versionId), "GetDiagramArtifact call with invalid versionId does not exit with 404 NotFoundException!");

            var serviceErrorMessage = Deserialization.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);

            // Validation: Exception should contain proper errorCode in the response content.
            Assert.That(serviceErrorMessage.ErrorCode.Equals(InternalApiErrorCodes.ItemNotFound), "GetDiagramArtifact with invalid versionId should return {0} errorCode but {1} is returned", ErrorCodes.ResourceNotFound, serviceErrorMessage.ErrorCode);
        }

        [TestCase(0)]
        [TestCase(-10)]
        [TestCase(999)]
        [TestRail(183028)]
        [Description("Create & publish a glossary artifact, Get GlossaryArtifact with invalid versionId.  Verify 404 NotFound.")]
        public void GetGlossaryArtifact_PublishAndGetGlossaryArtifactWithInvalidVersionId_404NotFound(int versionId)
        {
            // Setup: Create and publish a glossary artifact
            var publishedGlossaryArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Glossary);
            NovaGlossaryArtifact glossaryArtifact = null;

            // Execute: Get the glossary artifact with invalid versionId using GetGlossaryArtifact
            var ex = Assert.Throws<Http404NotFoundException>(() => glossaryArtifact = Helper.ArtifactStore.GetGlossaryArtifact(_user, publishedGlossaryArtifact.Id, versionId: versionId), "GetGlossaryArtifact call with invalid versionId does not exit with 404 NotFoundException!");

            var serviceErrorMessage = Deserialization.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);

            // Validation: Exception should contain proper errorCode in the response content.
            Assert.That(serviceErrorMessage.ErrorCode.Equals(InternalApiErrorCodes.ItemNotFound), "GetGlossaryArtifact with invalid versionId should return {0} errorCode but {1} is returned", ErrorCodes.ResourceNotFound, serviceErrorMessage.ErrorCode);
        }

        [TestCase(0)]
        [TestCase(-10)]
        [TestCase(999)]
        [TestRail(183029)]
        [Description("Create & publish a use case artifact, Get UseCaseArtifact with invalid versionId.  Verify 404 NotFound.")]
        public void GetUseCaseArtifact_PublishAndGetUseCaseArtifactWithInvalidVersionId_404NotFound(int versionId)
        {
            // Setup: Create and publish a use case artifact
            var publishedUseCaseArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);
            NovaUseCaseArtifact usecaseArtifact = null;

            // Execute: Get the use case artifact with invalid versionId using GetUseCaseArtifact
            var ex = Assert.Throws<Http404NotFoundException>(() => usecaseArtifact = Helper.ArtifactStore.GetUseCaseArtifact(_user, publishedUseCaseArtifact.Id, versionId: versionId), "GetUseCaseArtifact call with invalid versionId does not exit with 404 NotFoundException!");

            var serviceErrorMessage = Deserialization.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);

            // Validation: Exception should contain proper errorCode in the response content.
            Assert.That(serviceErrorMessage.ErrorCode.Equals(InternalApiErrorCodes.ItemNotFound), "GetUseCaseArtifact with invalid versionId should return {0} errorCode but {1} is returned", ErrorCodes.ResourceNotFound, serviceErrorMessage.ErrorCode);
        }

        #endregion 404 Not Found Tests

        #region Private Functions
        #endregion Private Functions
    }
}
