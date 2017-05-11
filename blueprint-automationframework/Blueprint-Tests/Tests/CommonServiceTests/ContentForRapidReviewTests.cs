﻿using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.NovaModel.Components.RapidReview;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;

namespace CommonServiceTests
{
    public class ContentForRapidReviewTests : TestBase
    {
        private const string DIAGRAM_PATH = RestPaths.Svc.Components.RapidReview.DIAGRAM_id_;
        private const string GLOSSARY_PATH = RestPaths.Svc.Components.RapidReview.GLOSSARY_id_;
        private const string USECASE_PATH = RestPaths.Svc.Components.RapidReview.USECASE_id_;
        private const string ARTIFACTS_PROPERTIES_PATH = RestPaths.Svc.Components.RapidReview.Artifacts.PROPERTIES;

        private IUser _user;
        private IProject _project;

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

        #endregion

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllDiagramArtifactTypesForNovaRestMethods))]
        [TestRail(107388)]
        [Description("Run:  'GET svc/components/RapidReview/diagram/{artifactId}'  with the ID of a diagram artifact.  Verify proper content for Diagram artifact is returned.")]
        public void GetDiagramContentForRapidReview_DiagramArtifacts_ReturnsDefaultDiagramContent(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, artifactType);
            RapidReviewDiagram diagramContent = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                diagramContent = Helper.SvcComponents.GetRapidReviewDiagramContent(_user, artifact.Id);
            }, "'GET {0}' should return 200 OK when a valid token is passed.", DIAGRAM_PATH);

            // Verify:
            Assert.That(diagramContent.DiagramType, Is.EqualTo(artifactType.ToString()).IgnoreCase, "Returned diagram type must be {0}, but it is {1}", artifactType, diagramContent.DiagramType);
            Assert.AreEqual(artifact.Id, diagramContent.Id, "Returned properties must have artifact Id {0}, but it is {1}", artifact.Id, diagramContent.Id);
            Assert.IsEmpty(diagramContent.Shapes, "Newly created {0} shouldn't have any shapes, but it has {1} shapes.", artifactType, diagramContent.Shapes.Count);
            Assert.IsEmpty(diagramContent.Connections, "Newly created {0} shouldn't have any connections, but it has {1} connections.", artifactType, diagramContent.Connections.Count);
            // TODO: add assertions about diagram size
        }

        [TestCase]
        [TestRail(107389)]
        [Description("Run:  'GET svc/components/RapidReview/glossary/{artifactId}'  with the ID of the glossary artifact.  Verify proper artifact content is returned.")]
        public void GetGlossaryContentForRapidReview_GlossaryArtifact_ReturnsDefaultGlossaryContent()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Glossary);
            RapidReviewGlossary glossaryContent = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                glossaryContent = Helper.SvcComponents.GetRapidReviewGlossaryContent(_user, artifact.Id);
            }, "'GET {0}' should return 200 OK when a valid token is passed.", GLOSSARY_PATH);

            // Verify:
            Assert.AreEqual(artifact.Id, glossaryContent.Id, "Returned properties must have artifact Id {0}, but it is {1}", artifact.Id, glossaryContent.Id);
            Assert.IsEmpty(glossaryContent.Terms, "Newly created Glossary shouldn't have any terms, but it has {0} terms.", glossaryContent.Terms.Count);
        }

        [TestCase]
        [TestRail(107390)]
        [Description("Run:  svc/components/RapidReview/usecase/{artifactId}  with the ID of the Use Case artifact.  Verify proper artifact content is returned.")]
        public void GetUseCaseContentForRapidReview_UseCaseArtifact_ReturnsDefaultArtifactContent()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.UseCase);
            UseCase artifactContent = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactContent = Helper.SvcComponents.GetRapidReviewUseCaseContent(_user, artifact.Id);
            }, "'GET {0}' should return 200 OK when a valid token is passed.", USECASE_PATH);

            // Verify:
            Assert.AreEqual(artifact.Id, artifactContent.Id, "Returned properties must have artifact Id {0}, but it is {1}", artifact.Id, artifactContent.Id);
            Assert.AreEqual(1, artifactContent.Steps.Count, "Newly created Use Case must have 1 step, but it has {0}", artifactContent.Steps.Count);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForNovaRestMethods))]
        [TestRail(107391)]
        [Description("Run:  svc/components/RapidReview/artifacts/properties  and pass the ID of an artifact in the request body.  Verify properties of the artifact are returned.")]
        public void GetPropertiesForRapidReview_SingleArtifact_ReturnsArtifactProperties(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, artifactType);
            RapidReviewProperties propertiesContent = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                propertiesContent = Helper.SvcComponents.GetRapidReviewArtifactsProperties(_user,
                    new List<int>() { artifact.Id } );
            }, "'GET {0}' should return 200 OK when a valid token is passed.", ARTIFACTS_PROPERTIES_PATH);

            // Verify:
            Assert.AreEqual(artifact.Id, propertiesContent.ArtifactId, "Returned properties must have artifact Id {0}, but it is {1}", artifact.Id, propertiesContent.ArtifactId);
            Assert.AreEqual(_user.DisplayName, propertiesContent.AuthorHistory[0].Value, "Returned properties must have Author {0}, but it is {1}",
                _user.DisplayName, propertiesContent.AuthorHistory[0].Value);
            Assert.IsNotEmpty(propertiesContent.Properties, "No properties were returned for the {0} artifact!", artifactType);
        }
    }
}
