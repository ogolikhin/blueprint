using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ExpandedArtifactTreeTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;

        #region Setup and Teardown

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Teardown

        [TestCase(BaseArtifactType.UseCase, BaseArtifactType.UseCase, BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.PrimitiveFolder, // PrimitiveFolders can only have projects as parents.
            BaseArtifactType.Actor,
            BaseArtifactType.BusinessProcess,
            BaseArtifactType.Document,
            BaseArtifactType.DomainDiagram,
            BaseArtifactType.GenericDiagram,
            BaseArtifactType.Glossary,
            BaseArtifactType.Process,
            BaseArtifactType.Storyboard,
            BaseArtifactType.TextualRequirement,
            BaseArtifactType.UIMockup,
            BaseArtifactType.UseCase,
            BaseArtifactType.UseCaseDiagram)]
        [TestRail(164525)]
        [Description("Create a chain of published parent/child artifacts and other top level artifacts.  GetExpandedArtifactTree with the ID of the artifact at the bottom of the chain." +
                     "Verify a list of top level artifacts is returned and only one has children.")]
        public void GetExpandedArtifactTree_LastPublishedArtifactInChain_ReturnsExpectedArtifactHierarchy(params BaseArtifactType[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            // Setup:
            var artifactChain = new List<IArtifact>();
            IArtifact bottomArtifact = null;

            // Create artifact chain.
            foreach (BaseArtifactType artifactType in artifactTypeChain)
            {
                bottomArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, parent: bottomArtifact);
                artifactChain.Add(bottomArtifact);
            }

            // Create some other top-level artifacts not part of the chain.
            var otherTopLevelArtifacts = new List<IArtifact>();
            otherTopLevelArtifacts.Add(Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor));
            otherTopLevelArtifacts.Add(Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process));

            // Execute:
            List<INovaArtifact> artifacts = null;

            Assert.DoesNotThrow(() => artifacts = Helper.ArtifactStore.GetExpandedArtifactTree(_user, _project, bottomArtifact.Id),
                "'GET {0}' should return 200 OK when passed valid parameters!", RestPaths.Svc.ArtifactStore.Projects_id_.ARTIFACTS_id_);

            // Verify:
            VerifyArtifactTree(artifactChain, artifacts);
            VerifyOtherTopLevelArtifactsExist(otherTopLevelArtifacts, artifacts);
        }

        [TestCase(BaseArtifactType.Actor, BaseArtifactType.Actor, BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.PrimitiveFolder, // PrimitiveFolders can only have projects as parents.
            BaseArtifactType.Actor,
            BaseArtifactType.BusinessProcess,
            BaseArtifactType.Document,
            BaseArtifactType.DomainDiagram,
            BaseArtifactType.GenericDiagram,
            BaseArtifactType.Glossary,
            BaseArtifactType.Process,
            BaseArtifactType.Storyboard,
            BaseArtifactType.TextualRequirement,
            BaseArtifactType.UIMockup,
            BaseArtifactType.UseCase,
            BaseArtifactType.UseCaseDiagram)]
        [TestRail(164526)]
        [Description("Create a chain of saved parent/child artifacts and other top level artifacts.  GetExpandedArtifactTree with the ID of the artifact at the bottom of the chain." +
            "Verify a list of top level artifacts is returned and only one has children.")]
        public void GetExpandedArtifactTree_LastSavedArtifactInChain_ReturnsExpectedArtifactHierarchy(params BaseArtifactType[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            // Setup:
            var artifactChain = new List<IArtifact>();
            IArtifact bottomArtifact = null;

            // Create artifact chain.
            foreach (BaseArtifactType artifactType in artifactTypeChain)
            {
                bottomArtifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType, parent: bottomArtifact);
                artifactChain.Add(bottomArtifact);
            }

            // Create some other top-level artifacts not part of the chain.
            Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);

            // Execute:
            List<INovaArtifact> artifacts = null;

            Assert.DoesNotThrow(() => artifacts = Helper.ArtifactStore.GetExpandedArtifactTree(_user, _project, bottomArtifact.Id),
                "'GET {0}' should return 200 OK when passed valid parameters!", RestPaths.Svc.ArtifactStore.Projects_id_.ARTIFACTS_id_);

            // Verify:
            VerifyArtifactTree(artifactChain, artifacts);
        }

        [TestCase(1, BaseArtifactType.Actor, BaseArtifactType.Actor, BaseArtifactType.Actor)]
        [TestCase(5, BaseArtifactType.PrimitiveFolder, // PrimitiveFolders can only have projects as parents.
            BaseArtifactType.Actor,
            BaseArtifactType.BusinessProcess,
            BaseArtifactType.Document,
            BaseArtifactType.DomainDiagram,
            BaseArtifactType.GenericDiagram,
            BaseArtifactType.Glossary,
            BaseArtifactType.Process,
            BaseArtifactType.Storyboard,
            BaseArtifactType.TextualRequirement,
            BaseArtifactType.UIMockup,
            BaseArtifactType.UseCase,
            BaseArtifactType.UseCaseDiagram)]
        [TestRail(164527)]
        [Description("Create a chain of saved parent/child artifacts and other top level artifacts.  GetExpandedArtifactTree with the ID of an artifact in the middle of the chain." +
            "Verify a list of top level artifacts is returned and only one has children.")]
        public void GetExpandedArtifactTree_NotLastSavedArtifactInChain_ReturnsExpectedArtifactHierarchy(int artifactIndex, params BaseArtifactType[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            // Setup:
            var artifactChain = new List<IArtifact>();
            IArtifact bottomArtifact = null;

            // Create artifact chain.
            foreach (BaseArtifactType artifactType in artifactTypeChain)
            {
                bottomArtifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType, parent: bottomArtifact);
                artifactChain.Add(bottomArtifact);
            }

            // Create some other top-level artifacts not part of the chain.
            Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);

            // Execute:
            List<INovaArtifact> artifacts = null;

            Assert.DoesNotThrow(() => artifacts = Helper.ArtifactStore.GetExpandedArtifactTree(_user, _project, artifactChain[artifactIndex].Id),
                "'GET {0}' should return 200 OK when passed valid parameters!", RestPaths.Svc.ArtifactStore.Projects_id_.ARTIFACTS_id_);

            // Verify:
            VerifyArtifactTree(artifactChain, artifacts, artifactIndex);
        }

        #region Private functions

        /// <summary>
        /// Verify that all other top level artifacts exist in the list of Nova artifacts with the same properties.
        /// </summary>
        /// <param name="otherTopLevelArtifacts">The list of other top level artifacts that aren't part of the artifact chain.</param>
        /// <param name="novaArtifacts">The list of NovaArtifacts returned by the GetExpandedArtifactTree call.</param>
        private static void VerifyOtherTopLevelArtifactsExist(List<IArtifact> otherTopLevelArtifacts, List<INovaArtifact> novaArtifacts)
        {
            foreach (IArtifact artifact in otherTopLevelArtifacts)
            {
                INovaArtifact novaArtifact = novaArtifacts.Find(a => a.Id == artifact.Id);

                Assert.NotNull(novaArtifact, "Couldn't find Artifact ID {0} in the list of Nova Artifacts!", artifact.Id);
                novaArtifact.AssertEquals(artifact, shouldCompareVersions: false);
            }
        }

        /// <summary>
        /// Verifies that all top level Nova artifacts have the correct values for Children & ParentId, and verifies the children of the one
        /// artifact chain that should exist.
        /// </summary>
        /// <param name="artifactChain">The list of parent/child artifacts we created.</param>
        /// <param name="novaArtifacts">The list of NovaArtifacts returned by the GetExpandedArtifactTree call.</param>
        /// <param name="artifactIndex">(optional) The index of the artifact chain containing the artifact whose tree we requested in the test.
        /// By default the entire chain is assumed to be returned from GetExpandedArtifactTree.</param>
        private void VerifyArtifactTree(List<IArtifact> artifactChain, List<INovaArtifact> novaArtifacts, int artifactIndex = int.MaxValue)
        {
            INovaArtifact topArtifact = VerifyAllTopLevelArtifacts(artifactChain, novaArtifacts);
            VerifyChildArtifactsInChain(artifactChain, topArtifact, artifactIndex);
        }

        /// <summary>
        /// Verifies that the ParentId of all top level Nova artifacts is the ProjectId, and that none of the Nova artifacts has Children except
        /// the top level artifact in the chain we created.
        /// </summary>
        /// <param name="artifactChain">The list of parent/child artifacts we created.</param>
        /// <param name="novaArtifacts">The list of NovaArtifacts returned by the GetExpandedArtifactTree call.</param>
        /// <returns>The NovaArtifact corresponding to the top level artifact in the chain we created.</returns>
        private INovaArtifact VerifyAllTopLevelArtifacts(List<IArtifact> artifactChain, List<INovaArtifact> novaArtifacts)
        {
            IArtifact topArtifact = artifactChain[0];
            INovaArtifact topNovaArtifact = null;

            // Make sure only the top artifact has children and that all top-level artifacts have parentId == projectId.
            foreach (INovaArtifact artifact in novaArtifacts)
            {
                if (artifact.Id == topArtifact.Id)
                {
                    topNovaArtifact = artifact;
                    Assert.NotNull(artifact.Children,
                        "The Children property of artifact '{0}' should not be null because it's the top level in the artifact chain we created!",
                        artifact.Id);
                }
                else
                {
                    Assert.IsNull(artifact.Children,
                        "The Children property of artifact '{0}' should be null because it's not in the artifact chain we created!",
                        artifact.Id);
                }

                Assert.AreEqual(_project.Id, artifact.ParentId, "The parent of all top level artifacts should be the project ID!");
            }

            return topNovaArtifact;
        }

        /// <summary>
        /// Verifies that children of the Nova artifact equal it's corresponding artifact in the chain we created and that the bottom Nova artifact
        /// doesn't have any children in the Children property.
        /// </summary>
        /// <param name="artifactChain">The list of parent/child artifacts we created.</param>
        /// <param name="topNovaArtifact">The top level Nova artifact returned by the GetExpandedArtifactTree call that matches the top level artifact we created.</param>
        /// <param name="artifactIndex">(optional) The index of the artifact chain containing the artifact whose tree we requested in the test.
        /// By default the entire chain is assumed to be returned from GetExpandedArtifactTree.</param>
        private static void VerifyChildArtifactsInChain(List<IArtifact> artifactChain, INovaArtifact topNovaArtifact, int artifactIndex = int.MaxValue)
        {
            // Verify all the children in the chain.
            INovaArtifact currentChild = topNovaArtifact;

            foreach (IArtifact artifact in artifactChain)
            {
                Assert.NotNull(currentChild, "No NovaArtifact was returned that matches with artifact '{0}' that we created!", artifact.Id);

                currentChild.AssertEquals(artifact, shouldCompareVersions: false);
                currentChild = currentChild.Children?[0];

                if (--artifactIndex < 0)
                {
                    break;
                }
            }

            Assert.IsNull(currentChild, "The Children property of the last artifactin the chain should be null!");
        }

        #endregion Private functions
    }
}
