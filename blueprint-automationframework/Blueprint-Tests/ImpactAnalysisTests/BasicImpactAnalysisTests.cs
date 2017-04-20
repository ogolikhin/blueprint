using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using Model.NovaModel.Components.ImpactAnalysisService;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;

namespace ImpactAnalysisTests
{
    [TestFixture]
    [Category(Categories.ImpactAnalysis)]
    class BasicImpactAnalysisTests : TestBase
    {
        private IUser _user = null;
        private List<IProject> _projects = null;
        private IProject _project = null;

        private const string SVC_PATH = RestPaths.ImpactAnalysis.IMPACT_ANALYSIS;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_user, shouldRetrievePropertyTypes: true);
            _project = _projects[0];
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region Positive tests

        [TestCase(TraceDirection.To)]
        [TestCase(TraceDirection.From)]
        [TestCase(TraceDirection.TwoWay)]
        [TestRail(290240)]
        [Description("Create and publish artifact with traces to target artifacts one of them with suspect flag.  " +
            "Verify that GetImpactAnalysis call returns correct suspect flags for artifacts.")]
        public void GetImpactAnalysisInfo_SuspectTrace_ReturnsCorrectRelationship(TraceDirection direction)
        {
            // Setup:
            const int LEVEL = 1;
            var bpServerAddress = Helper.BlueprintServer.Address;
            var targetArtifact1 = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var targetArtifact2 = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UIMockup);

            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Document);
            OpenApiArtifact.AddTrace(bpServerAddress, sourceArtifact, targetArtifact1, direction, _user);
            OpenApiArtifact.AddTrace(bpServerAddress, sourceArtifact, targetArtifact2, direction, _user, isSuspect: true);    
            sourceArtifact.Publish();
            
            // Execute:
            ImpactAnalysisResult impactAnalysisInfo = null;
            var path = I18NHelper.FormatInvariant(SVC_PATH, sourceArtifact.Id, LEVEL);
            Assert.DoesNotThrow(() => impactAnalysisInfo =
                ImpactAnalysis.GetImpactAnalysis(_user, Helper.ArtifactStore.Address, sourceArtifact.Id, LEVEL),
                "'GET {0}' should return 200 OK when valid parameters are passed.", path);

            // Verify:
            Assert.IsNotNull(impactAnalysisInfo, "ImpactAnalysis information was not returned!");
            Assert.AreEqual(2, impactAnalysisInfo.Tree.Root.Nodes.Count, "Amount of nodes is different from 2!");

            Assert.AreEqual(false, impactAnalysisInfo.Tree.Root.Nodes[0].IsSuspect, "IsSuspect flag is set to true!");
            Assert.AreEqual(true, impactAnalysisInfo.Tree.Root.Nodes[1].IsSuspect, "IsSuspect flag is set to false!");
        }

        [TestCase(TraceDirection.TwoWay)]
        [TestRail(290241)]
        [Description("Create and publish artifact with trace to target artifact.  User does not have permissions to target artifact.  " +
            "Verify that GetImpactAnalysis call returns correct data.")]
        public void GetImpactAnalysisInfo_NoPemissionsToTargetArtifact_ReturnsCorrectRelationship(TraceDirection direction)
        {
            // Setup:
            const int LEVEL = 1;

            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            var user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(user, TestHelper.ProjectRole.None, _project, targetArtifact);

            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Document);
            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, targetArtifact, direction, _user);
            sourceArtifact.Publish();

            // Execute:
            ImpactAnalysisResult impactAnalysisInfo = null;
            var path = I18NHelper.FormatInvariant(SVC_PATH, sourceArtifact.Id, LEVEL);
            Assert.DoesNotThrow(() => impactAnalysisInfo =
                ImpactAnalysis.GetImpactAnalysis(user, Helper.ArtifactStore.Address, sourceArtifact.Id, LEVEL),
                "'GET {0}' should return 200 OK when valid parameters are passed.", path);

            // Verify:
            Assert.IsNotNull(impactAnalysisInfo, "ImpactAnalysis information was not returned!");
            Assert.AreEqual(1, impactAnalysisInfo.Tree.Root.Nodes.Count, "Amount of nodes is different from 1!");
            Assert.IsNull(impactAnalysisInfo.Tree.Root.Nodes[0].Name, "The Name is not null!");
            Assert.IsNull(impactAnalysisInfo.Tree.Root.Nodes[0].Prefix, "The Prefix is not null");
            Assert.AreEqual(false, impactAnalysisInfo.Tree.Root.Nodes[0].IsSuspect);
        }

        #endregion Positive tests

        #region Negative tests

        [TestCase(TraceDirection.To)]
        [TestRail(290242)]
        [Description("Create and publish artifact with trace to target artifact.  Call ImpactAnalysis with invalid level.  " + 
            "Verify that GetImpactAnalysis call returns 400 Bad Request.")]
        public void GetImpactAnalysisInfo_InvalidLevel_Returns400BadRequest(TraceDirection direction)
        {
            // Setup:
            const int LEVEL = 0;

            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Document);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, targetArtifact, direction, _user);
            sourceArtifact.Publish();

            // Execute:
            var path = I18NHelper.FormatInvariant(SVC_PATH, sourceArtifact.Id, LEVEL);
            var ex = Assert.Throws<Http400BadRequestException>(() =>
                ImpactAnalysis.GetImpactAnalysis(_user, Helper.ArtifactStore.Address, sourceArtifact.Id, LEVEL),
                "'GET {0}' should return 400 Bad Request when invalid level passed.", path);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ImpactAnalysisInvalidLevel,
                "The level specified for Impact Analysis is invalid.");
        }

        [TestCase(TraceDirection.TwoWay)]
        [TestRail(290243)]
        [Description("Create and publish artifact with trace to target artifact.  User does not have permissions to source artifact.  " +
            "Verify that GetImpactAnalysis call returns 404 Not Found")]
        public void GetImpactAnalysisInfo_NoPermissionsToSource_404NotFound(TraceDirection direction)
        {
            // Setup:
            const int LEVEL = 1;

            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Document);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            var user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(user, TestHelper.ProjectRole.None, _project, sourceArtifact);

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, targetArtifact, direction, _user);
            sourceArtifact.Publish();

            // Execute:
            var path = I18NHelper.FormatInvariant(SVC_PATH, sourceArtifact.Id, LEVEL);
            var ex = Assert.Throws<Http404NotFoundException>(() =>
                ImpactAnalysis.GetImpactAnalysis(user, Helper.ArtifactStore.Address, sourceArtifact.Id, LEVEL),
                "'GET {0}' should return 404 Not Found when user does not have permissions to source artifact.", path);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden,
                "You have attempted to access an item that does not exist or you do not have permission to view.");
        }

        [TestCase(TraceDirection.From)]
        [TestRail(290244)]
        [Description("Call ImpactAnalysis for non-existing artifact.  Verify that GetImpactAnalysis call returns 404 Not Found.")]
        public void GetImpactAnalysisInfo_NonExistingArtifact_Returns404NotFound(TraceDirection direction)
        {
            // Setup:
            const int LEVEL = 1;
            const int NOT_EXISTING_ID = int.MaxValue;

            // Execute:
            var path = I18NHelper.FormatInvariant(SVC_PATH, NOT_EXISTING_ID, LEVEL);
            var ex = Assert.Throws<Http404NotFoundException>(() =>
                ImpactAnalysis.GetImpactAnalysis(_user, Helper.ArtifactStore.Address, NOT_EXISTING_ID, LEVEL),
                "'GET {0}' should return 404 Not Found when source artifact is not exist.", path);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an item that does not exist or you do not have permission to view.");
        }

        #endregion Negative tests
    }
}