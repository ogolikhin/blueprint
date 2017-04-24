using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.ModelHelpers;
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

        [TestCase]
        [TestRail(303035)]
        [Description("Create and publish an artifact.  Verify that GetImpactAnalysis call returns only root node.")]
        public void GetImpactAnalysisInfo_ArtifactWithNoTraces_ReturnsCorrectRelationship()
        {
            // Setup:
            const int LEVEL = 1;

            var sourceArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Document);

            // Execute:
            ImpactAnalysisResult impactAnalysisInfo = null;
            var path = I18NHelper.FormatInvariant(SVC_PATH, sourceArtifact.Id, LEVEL);
            Assert.DoesNotThrow(() => impactAnalysisInfo = Helper.ImpactAnalysis.GetImpactAnalysis(_user, sourceArtifact.Id, LEVEL),
                "'GET {0}' should return 200 OK when valid parameters are passed.", path);

            // Verify:
            Assert.IsNotNull(impactAnalysisInfo, "ImpactAnalysis information was not returned!");
            Assert.AreEqual(0, impactAnalysisInfo.Tree.Root.Nodes.Count, "Amount of nodes is different from 0!");
        }

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

            var sourceArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Document);
            var targetArtifact1 = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);
            var targetArtifact2 = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.UIMockup);

            sourceArtifact.Lock(_user);
            var trace1 = ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_user, sourceArtifact.Id, targetArtifact1.Id, (int)targetArtifact1.ProjectId,
                ChangeType.Create, Helper.ArtifactStore, direction);
            var trace2 = ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_user, sourceArtifact.Id, targetArtifact2.Id, (int)targetArtifact2.ProjectId,
                ChangeType.Create, Helper.ArtifactStore, direction, isSuspect: true);
            sourceArtifact.Publish(_user);

            // Execute:
            ImpactAnalysisResult impactAnalysisInfo = null;
            var path = I18NHelper.FormatInvariant(SVC_PATH, sourceArtifact.Id, LEVEL);
            Assert.DoesNotThrow(() => impactAnalysisInfo = Helper.ImpactAnalysis.GetImpactAnalysis(_user, sourceArtifact.Id, LEVEL),
                "'GET {0}' should return 200 OK when valid parameters are passed.", path);

            // Verify:
            Assert.IsNotNull(impactAnalysisInfo, "ImpactAnalysis information was not returned!");
            Assert.AreEqual(2, impactAnalysisInfo.Tree.Root.Nodes.Count, "Amount of nodes is different from 2!");

            VerifyImpactAnalysisResultNode(impactAnalysisInfo.Tree.Root.Nodes[0], trace1, targetArtifact1);
            VerifyImpactAnalysisResultNode(impactAnalysisInfo.Tree.Root.Nodes[1], trace2, targetArtifact2);
        }

        [TestCase(TraceDirection.TwoWay)]
        [TestRail(290241)]
        [Description("Create and publish artifact with trace to target artifact.  User does not have permissions to target artifact.  " +
            "Verify that GetImpactAnalysis call returns correct data.")]
        public void GetImpactAnalysisInfo_NoPermissionsToTargetArtifact_ReturnsCorrectRelationship(TraceDirection direction)
        {
            // Setup:
            const int LEVEL = 1;

            var sourceArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Document);
            var targetArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);

            var userWithoutPermissionToTarget = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutPermissionToTarget, TestHelper.ProjectRole.None, _project, targetArtifact);

            sourceArtifact.Lock(_user);
            var trace = ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_user, sourceArtifact.Id, targetArtifact.Id, (int)targetArtifact.ProjectId,
                ChangeType.Create, Helper.ArtifactStore, direction, isSuspect: true);
            sourceArtifact.Publish(_user);

            // Execute:
            ImpactAnalysisResult impactAnalysisInfo = null;
            var path = I18NHelper.FormatInvariant(SVC_PATH, sourceArtifact.Id, LEVEL);
            Assert.DoesNotThrow(() => impactAnalysisInfo = Helper.ImpactAnalysis.GetImpactAnalysis(userWithoutPermissionToTarget, sourceArtifact.Id, LEVEL),
                "'GET {0}' should return 200 OK when valid parameters are passed.", path);

            // Verify:
            Assert.IsNotNull(impactAnalysisInfo, "ImpactAnalysis information was not returned!");
            Assert.AreEqual(1, impactAnalysisInfo.Tree.Root.Nodes.Count, "Amount of nodes is different from 1!");

            VerifyImpactAnalysisResultNode(impactAnalysisInfo.Tree.Root.Nodes[0], trace, targetArtifact, isUnauthorized: true);
        }

        #endregion Positive tests

        #region Negative tests

        [TestCase(0)]
        [TestCase(6)]
        [TestRail(290242)]
        [Description("Create and publish artifact with trace to target artifact.  Call ImpactAnalysis with invalid level.  " + 
            "Verify that GetImpactAnalysis call returns 400 Bad Request.")]
        public void GetImpactAnalysisInfo_InvalidLevel_Returns400BadRequest(int level)
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Document);
            var targetArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);

            sourceArtifact.Lock(_user);
            ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_user, sourceArtifact.Id, targetArtifact.Id, (int)targetArtifact.ProjectId, ChangeType.Create,
                Helper.ArtifactStore, TraceDirection.To);
            sourceArtifact.Publish(_user);

            // Execute:
            var path = I18NHelper.FormatInvariant(SVC_PATH, sourceArtifact.Id, level);
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.ImpactAnalysis.GetImpactAnalysis(_user, sourceArtifact.Id, level),
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

            var sourceArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Document);
            var targetArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);

            var userWithoutPermissionToSource= Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutPermissionToSource, TestHelper.ProjectRole.None, _project, sourceArtifact);

            sourceArtifact.Lock(_user);
            ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_user, sourceArtifact.Id, targetArtifact.Id, (int)targetArtifact.ProjectId, ChangeType.Create,
                Helper.ArtifactStore, direction);
            sourceArtifact.Publish(_user);

            // Execute:
            var path = I18NHelper.FormatInvariant(SVC_PATH, sourceArtifact.Id, LEVEL);
            var ex = Assert.Throws<Http404NotFoundException>(() =>
                Helper.ImpactAnalysis.GetImpactAnalysis(userWithoutPermissionToSource, sourceArtifact.Id, LEVEL),
                "'GET {0}' should return 404 Not Found when user does not have permissions to source artifact.", path);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden,
                "You have attempted to access an item that does not exist or you do not have permission to view.");
        }

        [TestCase]
        [TestRail(290244)]
        [Description("Call ImpactAnalysis for non-existing artifact.  Verify that GetImpactAnalysis call returns 404 Not Found.")]
        public void GetImpactAnalysisInfo_NonExistingArtifact_Returns404NotFound()
        {
            // Setup:
            const int LEVEL = 1;
            const int NOT_EXISTING_ID = int.MaxValue;

            // Execute:
            var path = I18NHelper.FormatInvariant(SVC_PATH, NOT_EXISTING_ID, LEVEL);
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ImpactAnalysis.GetImpactAnalysis(_user, NOT_EXISTING_ID, LEVEL),
                "'GET {0}' should return 404 Not Found when source artifact is not exist.", path);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an item that does not exist or you do not have permission to view.");
        }

        #endregion Negative tests

        #region Private funactions

        /// <summary>
        /// Verifies impact analysis node from ImpactAnalysisResult against the actual trace & artifact
        /// </summary>
        /// <param name="node">ImpactAnalysis node</param>
        /// <param name="trace">Actual trace to check with</param>
        /// <param name="traceArtifact">Actual artifact to which trace pointing</param>
        /// <param name="isUnauthorized">(optional)Flag to set if artifact is accessible to the user. By default artifact is accessible</param>
        private void VerifyImpactAnalysisResultNode(ImpactAnalysisNode node, NovaTrace trace, ArtifactWrapper traceArtifact, bool isUnauthorized = false)
        {
            ThrowIf.ArgumentNull(trace, nameof(trace));
            ThrowIf.ArgumentNull(node, nameof(node));

            if (isUnauthorized)
            {
                Assert.AreEqual(0, node.Id, "Id property should be 0 when user does not have access to artifact!");
                Assert.IsNull(node.Name, "Name property should be null when user does not have access to artifact!");
                Assert.IsNull(node.Prefix, "Prefix property should be null when user does not have access to artifact!");
                Assert.AreEqual(0, node.TypeId, "TypeId property should be 0 when user does not have access to artifact!");
                Assert.IsFalse(node.IsSuspect, "IsSuspect property should be false when user does not have access to artifact!");
                Assert.IsTrue(node.IsUnauthorized, "IsUnauthorized property should be true when user does not have access to artifact!");
            }
            else
            {
                Assert.AreEqual(trace.ArtifactId, node.Id, "Trace ArtifactId property doesn't match to node Id property!");
                Assert.AreEqual(traceArtifact.Name, node.Name, "Trace artifact Name property doesn't match node Name property");
                Assert.AreEqual(traceArtifact.Prefix, node.Prefix, "Trace artifact Prefix property doesn't match node Prefix property");
                Assert.AreEqual(traceArtifact.ItemTypeId, node.TypeId, "Trace artifact ItemTypeId property doesn't match node TypeId property");
                Assert.AreEqual(trace.IsSuspect, node.IsSuspect, "Trace IsSuspect property doesn't match to node IsSupect property!");
                Assert.IsFalse(node.IsUnauthorized, "IsUnauthorized property should be false when user does not have access to artifact!");
            }
            // TODO: IncludedIn, IsLoop, IsRoot, ParentId(Id of an artifact in parent node)
        }

        #endregion Private functions
    }
}