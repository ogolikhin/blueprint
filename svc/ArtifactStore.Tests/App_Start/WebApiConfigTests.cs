using System.Linq;
using System.Net.Http;
using System.Web.Http;
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
            config.AssertTotalRoutes(15, "Please update asserts in WebApiConfigTests when changing routes.");
            config.AssertAction<StatusController>("GetStatus", HttpMethod.Get, "status");
            config.AssertAction<StatusController>("GetStatusUpCheck", HttpMethod.Get, "status/upcheck");
            config.AssertAction<ArtifactController>("GetProjectChildren", HttpMethod.Get, "projects/1/children");
            config.AssertAction<ArtifactController>("GetArtifactChildren", HttpMethod.Get, "projects/1/artifacts/2/children");
            config.AssertAction<ArtifactController>("GetExpandedTreeToArtifact", HttpMethod.Get, "projects/1/artifacts?expandedToArtifactId=2&includeChildren=true");
            config.AssertAction<ProjectMetaController>("GetProjectTypes", HttpMethod.Get, "projects/1/meta/customtypes");
            config.AssertAction<DiscussionController>("GetDiscussions", HttpMethod.Get, "artifacts/1/discussions");
            config.AssertAction<DiscussionController>("GetReplies", HttpMethod.Get, "artifacts/1/discussions/1/replies");
            config.AssertAction<RelationshipsController>("GetRelationships", HttpMethod.Get, "artifacts/1/relationships");
            config.AssertAction<RelationshipsController>("GetRelationshipDetails", HttpMethod.Get, "artifacts/1/relationshipdetails");
            config.AssertAction<ArtifactController>("GetSubArtifactTreeAsync", HttpMethod.Get, "artifacts/1/subartifacts");
            config.AssertAction<ArtifactVersionsController>("GetVersionControlArtifactInfo", HttpMethod.Get, "artifacts/versionControlInfo/1");
            config.AssertAction<ArtifactController>("GetArtifactNavigationPath", HttpMethod.Get, "artifacts/1/navigationPath");
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
