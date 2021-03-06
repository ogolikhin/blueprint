﻿using System.Linq;
using System.Net.Http;
using System.Web.Http;
using ArtifactStore.Collections;
using ArtifactStore.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;

namespace ArtifactStore
{
    [TestClass]
    public class WebApiConfigTests
    {
        [TestMethod]
        public void Register_Always_RegistersCorrectRoutes()
        {
            // Arrange
            var config = new HttpConfiguration();

            // Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();

            // Assert
            config.AssertTotalRoutes(54, "Please update asserts in WebApiConfigTests when changing routes.");
            config.AssertAction<StatusController>("GetStatus", HttpMethod.Get, "status");
            config.AssertAction<StatusController>("GetStatusUpCheck", HttpMethod.Get, "status/upcheck");
            config.AssertAction<ArtifactController>("GetProjectChildren", HttpMethod.Get, "projects/1/children");
            config.AssertAction<ArtifactController>("GetArtifactChildren", HttpMethod.Get, "projects/1/artifacts/2/children");
            config.AssertAction<ArtifactController>("GetExpandedTreeToArtifact", HttpMethod.Get, "projects/1/artifacts?expandedToArtifactId=2&includeChildren=true");
            config.AssertAction<ProjectMetaController>("GetProjectTypes", HttpMethod.Get, "projects/1/meta/customtypes");
            config.AssertAction<ProjectMetaController>("GetProjectApprovalStatuses", HttpMethod.Get, "projects/1/meta/approvalstatus");
            config.AssertAction<DiscussionController>("GetDiscussions", HttpMethod.Get, "artifacts/1/discussions");
            config.AssertAction<DiscussionController>("GetReplies", HttpMethod.Get, "artifacts/1/discussions/1/replies");
            config.AssertAction<RelationshipsController>("GetRelationships", HttpMethod.Get, "artifacts/1/relationships");
            config.AssertAction<RelationshipsController>("GetRelationshipDetails", HttpMethod.Get, "artifacts/1/relationshipdetails");
            config.AssertAction<RelationshipsController>("GetReviews", HttpMethod.Get, "artifacts/1/reviews");
            config.AssertAction<ArtifactController>("GetSubArtifactTreeAsync", HttpMethod.Get, "artifacts/1/subartifacts");
            config.AssertAction<ArtifactVersionsController>("GetVersionControlArtifactInfo", HttpMethod.Get, "artifacts/versionControlInfo/1");
            config.AssertAction<ArtifactController>("GetArtifactNavigationPath", HttpMethod.Get, "artifacts/1/navigationPath");
            config.AssertAction<ArtifactController>("GetArtifactsAuthorHistories", HttpMethod.Post, "artifacts/authorHistories");
            config.AssertAction<ArtifactController>("GetBaselineInfo", HttpMethod.Post, "artifacts/baselineInfo");
            config.AssertAction<WorkflowController>("GetTransitions", HttpMethod.Get, "artifacts/1/transitions");
            config.AssertAction<WorkflowController>("GetStateForArtifact", HttpMethod.Get, "artifacts/1/state");
            config.AssertAction<WorkflowController>("ChangeStateForArtifact", HttpMethod.Post, "artifacts/1/state");
            config.AssertAction<ReviewContainersController>("GetReviewSummary", HttpMethod.Get, "containers/1");
            config.AssertAction<ReviewContainersController>("GetReviewSummaryMetrics", HttpMethod.Get, "containers/1/metrics");
            config.AssertAction<ReviewContainersController>("GetContentAsync", HttpMethod.Get, "containers/1/content");
            config.AssertAction<ReviewContainersController>("GetParticipantsAsync", HttpMethod.Get, "containers/1/participants");
            config.AssertAction<ReviewContainersController>("AddParticipantsToReview", HttpMethod.Put, "containers/1/participants");
            config.AssertAction<ReviewContainersController>("GetReviewArtifactStatusesByParticipantAsync", HttpMethod.Get, "containers/1/artifactreviewers/artifactId=1");
            config.AssertAction<ReviewContainersController>("GetTableOfContentAsync", HttpMethod.Get, "containers/1/toc");
            config.AssertAction<ReviewContainersController>("AddArtifactsToReview", HttpMethod.Put, "containers/1/content");
            config.AssertAction<ReviewContainersController>("GetReviewedArtifacts", HttpMethod.Get, "containers/1/artifacts");
            config.AssertAction<ReviewContainersController>("AssignApprovalRequiredToArtifactsAsync", HttpMethod.Put, "containers/1/artifacts/approval");
            config.AssertAction<ReviewContainersController>("AssignRoleToParticipantAsync", HttpMethod.Put, "containers/1/reviewers/approval");
            config.AssertAction<ReviewContainersController>("GetReviewArtifactIndex", HttpMethod.Get, "containers/1/index/1");
            config.AssertAction<ReviewContainersController>("GetReviewTableOfContentArtifactIndex", HttpMethod.Get, "containers/1/toc/index/1");
            config.AssertAction<ReviewContainersController>("UpdateReviewArtifactApprovalAsync", HttpMethod.Put, "containers/1/experience/approval");
            config.AssertAction<ReviewContainersController>("UpdateReviewArtifactsViewedAsync", HttpMethod.Put, "containers/1/artifacts/viewed");
            config.AssertAction<ReviewContainersController>("GetReviewParticipantArtifactStatsAsync", HttpMethod.Get, "containers/1/participants/1/artifactstats");
            config.AssertAction<VersionControlController>("PublishArtifacts", HttpMethod.Post, "artifacts/publish");
            config.AssertAction<ReviewContainersController>("RemoveArtifactsFromReviewAsync", HttpMethod.Post, "containers/1/artifacts/remove");
            config.AssertAction<ReviewContainersController>("UpdateReviewerStatusAsync", HttpMethod.Put, "containers/1/reviewer/status");
            config.AssertAction<ArtifactController>("GetProcessInformationAsync", HttpMethod.Post, "artifacts/processInfo");
            config.AssertAction<ArtifactController>("GetStandardArtifactTypes", HttpMethod.Get, "artifacts/standardartifacttypes?filter=1");
            config.AssertAction<ReviewContainersController>("RemoveParticipantsFromReviewAsync", HttpMethod.Post, "containers/1/participants/remove");
            config.AssertAction<ReviewContainersController>("GetReviewSettingsAsync", HttpMethod.Get, "containers/1/settings");
            config.AssertAction<ReviewContainersController>("UpdateReviewSettingsAsync", HttpMethod.Put, "containers/1/settings?autoSave=false");
            config.AssertAction<ReviewContainersController>("UpdateMeaningOfSignatureAsync", HttpMethod.Put, "containers/1/participants/meaningofsignatures");
            config.AssertAction<ArtifactController>("GetStandardProperties", HttpMethod.Post, "artifacts/standardproperties");
            config.AssertAction<CollectionsController>("GetArtifactsInCollectionAsync", HttpMethod.Get, "collections/1/artifacts");
            config.AssertAction<CollectionsController>("AddArtifactsToCollectionAsync", HttpMethod.Post, "collections/1/artifacts?add");
            config.AssertAction<CollectionsController>("RemoveArtifactsFromCollectionAsync", HttpMethod.Post, "collections/1/artifacts?remove");
            config.AssertAction<CollectionsController>("SaveColumnsSettingsAsync", HttpMethod.Post, "collections/1/settings/columns");
            config.AssertAction<CollectionsController>("GetColumnsAsync", HttpMethod.Get, "collections/1/settings/columns");
        }

        [TestMethod]
        public void Register_GetAndHeadMethods_HaveNoCacheAttribute()
        {
            // Arrange
            var config = new HttpConfiguration();

            // Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();

            // Assert
            config.AssertMethodAttributes(attr => attr.Any(a => a is HttpGetAttribute || a is HttpHeadAttribute) == attr.Any(a => a is NoCacheAttribute),
                "{0} is missing NoCacheAttribute.");
        }

        [TestMethod]
        public void Register_AllHttpMethods_HaveSessionRequiredOrNoSessionRequiredAttribute()
        {
            // Arrange
            var config = new HttpConfiguration();

            // Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();

            // Assert
            config.AssertMethodAttributes(attr => attr.Any(a => a is SessionAttribute || a is NoSessionRequiredAttribute),
                "{0} is missing SessionAttribute or NoSessionRequiredAttribute.");
        }
    }
}
