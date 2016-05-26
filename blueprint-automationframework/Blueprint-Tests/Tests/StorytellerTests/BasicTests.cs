﻿using System.Collections;
using System.Linq;
using CustomAttributes;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using Helper;
using System.Net;
using TestCommon;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class BasicTests : TestBase
    {
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

        #endregion Setup and Cleanup

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "processType")]
        [TestCase(5, 4, 2)]
        [Description("Get the default process after creating and saving a new process artifact.  Verify that the" +
                     "returned process has the same Id as the process artifact Id and that the numbers of " +
                     "shapes, links and property values are as expected.")]
        public void GetDefaultProcess_VerifyReturnedProcess(
            int defaultShapesCount, 
            int defaultLinksCount, 
            int defaultPropertyValuesCount)
        {
            var artifact = Helper.Storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _user);

            var returnedProcess = Helper.Storyteller.GetProcess(_user, artifact.Id);

            Assert.IsNotNull(returnedProcess, "The returned process was null.");

            Assert.That(returnedProcess.Id == artifact.Id,
                "The ID of the returned process was '{0}', but '{1}' was expected.", returnedProcess.Id, artifact.Id);
            Assert.That(returnedProcess.Shapes.Count == defaultShapesCount,
                "The number of shapes in a default process is {0} but {1} shapes were returned.", defaultShapesCount, returnedProcess.Shapes.Count);
            Assert.That(returnedProcess.Links.Count == defaultLinksCount,
                "The number of links in a default process is {0} but {1} links were returned.", defaultLinksCount, returnedProcess.Links.Count);
            Assert.That(returnedProcess.PropertyValues.Count == defaultPropertyValuesCount,
                "The number of property values in a default process is {0} but {1} property values were returned.", defaultPropertyValuesCount, returnedProcess.PropertyValues.Count);
        }

        [TestCase]
        public void GetProcesses_ReturnedListContainsCreatedProcess()
        {
            IArtifact artifact = Helper.Storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _user);
            List<IProcess> processList = null;

            Assert.DoesNotThrow(() =>
            {
                processList = (List<IProcess>)Helper.Storyteller.GetProcesses(_user, _project.Id);
            }, "GetProcesses must not return an error.");

            // Get returned process from list of processes
            var returnedProcess = processList.Find(p => p.Name == artifact.Name);

            Assert.IsNotNull(returnedProcess, "List of processes must have newly created process, but it doesn't.");
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.UIMockup)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.BusinessProcess)]
        [TestCase(BaseArtifactType.Document)]
        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.Glossary)]
        [TestCase(BaseArtifactType.Storyboard)]
        [TestCase(BaseArtifactType.TextualRequirement)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(102883)]
        [Description("Create artifact, save and publish it. Search created artifact by name within all projects. Search must return created artifact.")]
        public void GetSearchArtifactResultsAllProjects_ReturnedListContainsCreatedArtifact(BaseArtifactType artifactType)
        {
            //Create an artifact with ArtifactType and populate all required values without properties
            var artifact = Helper.CreateArtifact(_project, _user, artifactType);

            artifact.Save(_user);
            artifact.Publish(_user);

            IList<IArtifactBase> artifactsList = null;

            Assert.DoesNotThrow(() =>
            {
                artifactsList = Artifact.SearchArtifactsByName(address: Helper.Storyteller.Address, user: _user, searchSubstring: artifact.Name);
            }, "{0}.{1}() shouldn't throw an exception when passed valid parameters!", nameof(Artifact), nameof(Artifact.SearchArtifactsByName));

            Assert.IsTrue(artifactsList.Count > 0, "No artifacts were found after adding an artifact!");
        }

        [TestCase]
        [TestRail(102884)]
        [Description("Check that search artifact by name returns 10 artifacts only.")]
        public void GetSearchArtifactResults_ReturnedListHasExpectedLength()
        {
            //Create an artifact with ArtifactType and populate all required values without properties
            var artifactList = new List<IArtifact>();

            for (int i = 0; i < 12; i++)
            {
                var artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
                artifact.Save(_user);
                artifact.Publish(_user);
                artifactList.Add(artifact);
            }

            //Implementation of CreateArtifact uses Artifact_ prefix to name artifacts
            string searchString = "Artifact_";
            IList<IArtifactBase> searchResultList = null;

            Assert.DoesNotThrow(() =>
            {
                searchResultList = Artifact.SearchArtifactsByName(address: Helper.Storyteller.Address, user: _user, searchSubstring: searchString);
            }, "{0}.{1}() shouldn't throw an exception when passed valid parameters!", nameof(Artifact), nameof(Artifact.SearchArtifactsByName));

            Assert.IsTrue(searchResultList.Count == 10, "Search results must have 10 artifacts, but they have '{0}'.", searchResultList.Count);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.UIMockup)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.BusinessProcess)]
        [TestCase(BaseArtifactType.Document)]
        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.Glossary)]
        [TestCase(BaseArtifactType.Storyboard)]
        [TestCase(BaseArtifactType.TextualRequirement)]
        [TestRail(123257)]
        [Description("Create artifact, save and publish it. Search created artifact by name within the project where artifact was created. Search must return created artifact.")]
        public void GetSearchArtifactResultsForOneProject_ReturnedListContainsCreatedArtifact(BaseArtifactType artifactType)
        {
            //Create an artifact with ArtifactType and populate all required values without properties
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, artifactType);

            artifact.Save(_user);
            artifact.Publish(_user);

            try
            {
                Assert.DoesNotThrow(() =>
                {
                    var artifactsList = Artifact.SearchArtifactsByName(address: Helper.BlueprintServer.Address,
                        user: _user, searchSubstring: artifact.Name, project: _project);
                    Assert.IsTrue(artifactsList.Count > 0);
                }, "Couldn't find an artifact named '{0}'.", artifact.Name);
            }

            finally
            {
                artifact.Delete(_user);
                artifact.Publish(_user);
            }
        }

        [TestCase]
        [TestRail(107376)]
        [Description("Add a user task after an existing user task, discard")]
        public void DiscardArtifactAddedUserTask_VerifyResult()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _user);

            // Find the end shape
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);
            
            // Find the incoming link for the end shape
            var endIncomingLink = process.GetIncomingLinkForShape(endShape);

            Assert.IsNotNull(endIncomingLink, "Process link was not found.");

            // Add a user/system task immediately before the end shape
            process.AddUserAndSystemTask(endIncomingLink);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _user);
            var processArtifact = Helper.Storyteller.Artifacts.Where(artifact => artifact.Id == process.Id).First();
            
            List<NovaDiscardArtifactResult> discardResultList = null;
            string expectedMessage = "Successfully discarded";

            Assert.DoesNotThrow(() =>
            {
                discardResultList = processArtifact.NovaDiscard(_user);
            }, "Discard must return no errors.");

            Assert.AreEqual(expectedMessage, discardResultList[0].Message, "Returned message must be {0}, but {1} was returned",
                expectedMessage, discardResultList[0].Message);
            Assert.AreEqual(0, discardResultList[0].Result, "Returned code must be {0}, but {1} was returned",
                0, discardResultList[0].Result);
        }
    }
}
