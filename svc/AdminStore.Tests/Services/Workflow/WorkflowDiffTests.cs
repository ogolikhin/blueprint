using System.Collections.Generic;
using AdminStore.Models.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdminStore.Services.Workflow
{
    [TestClass]
    public class WorkflowDiffTests
    {
        // Bug https://jira.blueprintsys.net/browse/STOR-4145
        [TestMethod]
        public void DiffWorkflows_SameArtifactTypeDeletedAndAdded_SuccessfulDeletion()
        {
            // Arrange
            var currentWorkflow = new IeWorkflow
            {
                Projects = new List<IeProject>
                {
                    new IeProject
                    {
                        Id = 1,
                        ArtifactTypes = new List<IeArtifactType>
                        {
                            new IeArtifactType { Id = 11 }, // deleted
                            new IeArtifactType { Id = 12 } // unchanged
                        }
                    }
                }
            };

            var updatedWorkflow = new IeWorkflow
            {
                Projects = new List<IeProject>
                {
                    new IeProject
                    {
                        Id = 1,
                        ArtifactTypes = new List<IeArtifactType>
                        {
                            new IeArtifactType { Id = 12 } // unchanged
                        }
                    },
                    new IeProject
                    {
                        Id = 2,
                        ArtifactTypes = new List<IeArtifactType>
                        {
                            new IeArtifactType { Id = -11 }, // added
                            new IeArtifactType { Id = -13 } // added
                        }
                    }
                }
            };

            var wDiff = new WorkflowDiff();

            // Act
            var diff = wDiff.DiffWorkflows(updatedWorkflow, currentWorkflow);

            // Assert
            Assert.AreEqual(diff.UnchangedProjectArtifactTypes.Count, 1);
            Assert.AreEqual(diff.UnchangedProjectArtifactTypes[0].Key, currentWorkflow.Projects[0].Id.Value); // 1
            Assert.AreEqual(diff.UnchangedProjectArtifactTypes[0].Value.Id, currentWorkflow.Projects[0].ArtifactTypes[1].Id); // 12

            Assert.AreEqual(diff.AddedProjectArtifactTypes.Count, 2);
            Assert.AreEqual(diff.AddedProjectArtifactTypes[0].Key, updatedWorkflow.Projects[1].Id.Value); // 2
            Assert.AreEqual(diff.AddedProjectArtifactTypes[0].Value.Id, updatedWorkflow.Projects[1].ArtifactTypes[0].Id); // 11
            Assert.AreEqual(diff.AddedProjectArtifactTypes[1].Key, updatedWorkflow.Projects[1].Id.Value); // 2
            Assert.AreEqual(diff.AddedProjectArtifactTypes[1].Value.Id, updatedWorkflow.Projects[1].ArtifactTypes[1].Id); // 13

            Assert.AreEqual(diff.DeletedProjectArtifactTypes.Count, 1);
            Assert.AreEqual(diff.DeletedProjectArtifactTypes[0].Key, currentWorkflow.Projects[0].Id.Value); // 1
            Assert.AreEqual(diff.DeletedProjectArtifactTypes[0].Value.Id, currentWorkflow.Projects[0].ArtifactTypes[0].Id); // 11
        }
    }
}
