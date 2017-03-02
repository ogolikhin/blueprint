using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Model.StorytellerModel.Enums;
using Utilities;
using Utilities.Factories;

namespace Helper
{
    public static class StorytellerTestHelper
    {
        #region Public Methods

        /// <summary>
        /// Adds random link labels to the specified Process.  Note: Only links coming from Decision Points can have labels.
        /// </summary>
        /// <param name="storyteller">An IStoryteller instance to make REST calls to.</param>
        /// <param name="process">The Process whose link labels are to be updated.</param>
        /// <param name="user">(optional) The user to authenticate with.  Only needed if updateProcess is true.</param>
        /// <param name="updateProcess">(optional) Pass true to update the Process after changing the link labels.</param>
        /// <returns>The Process.</returns>
        public static IProcess AddRandomLinkLabelsToProcess(IStoryteller storyteller,
            IProcess process,
            IUser user = null,
            bool updateProcess = false)
        {
            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(process, nameof(process));

            foreach (var link in process.Links)
            {
                var sourceShape = process.GetProcessShapeById(link.SourceId);

                // Only links coming out of Decision shapes can have labels.
                if (sourceShape.IsTypeOf(ProcessShapeType.SystemDecision) || sourceShape.IsTypeOf(ProcessShapeType.UserDecision))
                {
                    link.Label = RandomGenerator.RandomAlphaNumeric(10);
                }
            }

            if (updateProcess)
            {
                process = storyteller.UpdateProcess(user, process);
            }

            return process;
        }

        /// <summary>
        /// Asserts that the two Processes are equal.
        /// </summary>
        /// <param name="expectedProcess">The expected process</param>
        /// <param name="actualProcess">The actual process being compared to the expected process</param>
        /// <param name="allowNegativeShapeIds">Allows for inequality of shape ids where a newly added shape has a negative id</param>
        /// <param name="isCopiedProcess">(optional) Flag indicating if the process being compared is a copied process</param>
        /// <exception cref="AssertionException">If expectedProcess is not equal to actualProcess</exception>
        /// <remarks>If 1 of the 2 processes being compared has negative Ids, that process must be the first parameter</remarks>
        public static void AssertProcessesAreEqual(IProcess expectedProcess, IProcess actualProcess, bool allowNegativeShapeIds = false, bool isCopiedProcess = false)
        {
            ThrowIf.ArgumentNull(expectedProcess, nameof(expectedProcess));
            ThrowIf.ArgumentNull(actualProcess, nameof(actualProcess));

            // Assert basic Process properties

            // Artifact ids of copied processes must be different
            if (isCopiedProcess)
            {
                Assert.AreNotEqual(expectedProcess.Id, actualProcess.Id, "The ids of copied processes must not match");
            }
            else
            {
                Assert.AreEqual(expectedProcess.Id, actualProcess.Id, "The ids of the processes don't match");
            }
            
            Assert.AreEqual(expectedProcess.Name, actualProcess.Name, "The names of the processes don't match");
            Assert.AreEqual(expectedProcess.BaseItemTypePredefined, actualProcess.BaseItemTypePredefined,
                "The base item types of the processes don't match");

            // A copied process does not necessarily have the same Project Id as it's source
            if (!isCopiedProcess)
            {
                Assert.AreEqual(expectedProcess.ProjectId, actualProcess.ProjectId, "The project ids of the processes don't match");
            }

            Assert.AreEqual(expectedProcess.TypePrefix, actualProcess.TypePrefix, "The type prefixes of the processes don't match");

            // Assert that Link counts, Shape counts, Property counts, and DecisionBranchDestinationLinks counts are equal
            Assert.AreEqual(expectedProcess.PropertyValues.Count, actualProcess.PropertyValues.Count,
                "The processes have different property counts");
            Assert.AreEqual(expectedProcess.Links.Count, actualProcess.Links.Count, "The processes have different link counts");
            Assert.AreEqual(expectedProcess.Shapes.Count, actualProcess.Shapes.Count,
                "The processes have different process shape counts");
 
            // TODO This is a quick fix for tests deleting only decision from the process model
            var process1DecisionBranchDestinationLinkCount = expectedProcess.DecisionBranchDestinationLinks?.Count ?? 0;
            var process2DecisionBranchDestinationLinkCount = actualProcess.DecisionBranchDestinationLinks?.Count ?? 0;

            Assert.AreEqual(process1DecisionBranchDestinationLinkCount, process2DecisionBranchDestinationLinkCount,
                "The processes have different decision branch destination link counts");

            // Assert that Process properties are equal
            foreach (var process1Property in expectedProcess.PropertyValues)
            {
                var process2Property = FindPropertyValue(process1Property.Key, actualProcess.PropertyValues);

                AssertPropertyValuesAreEqual(process1Property.Value, process2Property.Value);
            }


            // TODO: Develop a method to see if the links are equivalent
            // Copied processes do not have the same process links
            if (!isCopiedProcess)
            {
                // Assert that process links are the same
                // This involves finding the new id of shapes that had negative ids in the source process
                AssertLinksAreEqual(expectedProcess, actualProcess);
            }

            //Assert that Process shapes are equal
            foreach (var process1Shape in expectedProcess.Shapes)
            {
                var process2Shape = FindProcessShapeByName(process1Shape.Name, actualProcess.Shapes);

                AssertShapesAreEqual(process1Shape, process2Shape, allowNegativeShapeIds, isCopiedProcess);
            }
        }

        /// <summary>
        /// Asserts that a persona reference relationship is correct for a given actor
        /// </summary>
        /// <param name="relationship">The relationship being tested.</param>
        /// <param name="actorArtifactProject">The project where the actor artifact is located.</param>
        /// <param name="actorArtifact">The actor artifact</param>
        /// <param name="isReadOnlyExpected">(optional) Flag indicating whether the relationship is expected to be read-only (Default: false)</param>
        /// <param name="isSuspectExpected">(optional) Flag indicating whether the relationship is expected to be suspect (Default: false)</param>
        public static void AssertPersonaReferenceRelationshipIsCorrect(Relationship relationship, 
            IProject actorArtifactProject, 
            IArtifactBase actorArtifact,
            bool isReadOnlyExpected = false,
            bool isSuspectExpected = false
            )
        {
            ThrowIf.ArgumentNull(relationship, nameof(relationship));
            ThrowIf.ArgumentNull(actorArtifactProject, nameof(actorArtifactProject));
            ThrowIf.ArgumentNull(actorArtifact, nameof(actorArtifact));

            Assert.AreEqual(relationship.ArtifactId, actorArtifact.Id, 
                "The artifact id of the relationship {0} does not match the actor id {1}.", 
                relationship.ArtifactId, actorArtifact.Id);

            Assert.AreEqual(relationship.ArtifactName, actorArtifact.Name, 
                "The artifact name of the relationship {0} does not match the actor name {1}.", 
                relationship.ArtifactName, actorArtifact.Name);

            Assert.AreEqual(relationship.ItemId, actorArtifact.Id, 
                "The item id of the relationship {0} does not match the actor id {1}.", 
                relationship.ItemId, actorArtifact.Id);

            Assert.AreEqual(relationship.ItemName, actorArtifact.Name, 
                "The item name of the relationship {0} does not match the actor name {1}.", 
                relationship.ItemName, actorArtifact.Name);

            Assert.AreEqual((ItemTypePredefined)relationship.PrimitiveItemTypePredefined, ItemTypePredefined.Actor, 
                "The item type of the relationship {0} does not match {1}.", 
                relationship.PrimitiveItemTypePredefined, ItemTypePredefined.Actor);

            Assert.AreEqual(relationship.ProjectId, actorArtifact.ProjectId, 
                "The project id of the relationship {0} does not match the actor project id {1}.", 
                relationship.ProjectId, actorArtifact.ProjectId);

            Assert.AreEqual(relationship.ProjectName, actorArtifactProject.Name, 
                "The project name of the relationship {0} does not match the actor project name {1}.", 
                relationship.ProjectName, actorArtifactProject.Name);

            Assert.AreEqual(relationship.ReadOnly, isReadOnlyExpected, 
                "The actor artifact of the relationships has readonly as {0) but {1} is expected.", 
                relationship.ReadOnly, isReadOnlyExpected);

            Assert.AreEqual(relationship.IsSuspect, isSuspectExpected, 
                "The actor artifact of the relationships has suspect as {0) but {1} is expected.", 
                relationship.IsSuspect, isSuspectExpected);

            Assert.AreEqual(relationship.Direction, TraceDirection.To, 
                "The trace direction of the relationship, {0}, does not match the expected direction, {1}.", 
                relationship.Direction, TraceDirection.To);

            Assert.AreEqual(relationship.TraceType, LinkType.Association, 
                "The trace type of the relationship, {0}, does not match the expected type, {1}.", 
                relationship.TraceType, LinkType.Association);
        }

        /// <summary>
        /// Updates and verifies the process returned from UpdateProcess and GetProcess
        /// </summary>
        /// <param name="processToVerify">The process to verify</param>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="user">The user that updates the process</param>
        /// <returns> The process returned from Get Process </returns>
        public static IProcess UpdateAndVerifyProcess(IProcess processToVerify, IStoryteller storyteller, IUser user)
        {
            ThrowIf.ArgumentNull(processToVerify, nameof(processToVerify));
            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Update the process using UpdateProcess
            storyteller.UpdateProcess(user, processToVerify);

            // Get the process using GetProcess
            var processReturnedFromGet = storyteller.GetProcess(user, processToVerify.Id);

            Assert.IsNotNull(processReturnedFromGet, "GetProcess() returned a null process.");

            // Assert that the process returned from the GetProcess method is identical to the process sent with the UpdateProcess method
            // Allow negative shape ids in the process being verified
            AssertProcessesAreEqual(processToVerify, processReturnedFromGet, allowNegativeShapeIds: true);

            // Assert that the decision branch destination links are in sync during the get opertations
            AssertDecisionBranchDestinationLinksAreInsync(processReturnedFromGet);

            return processReturnedFromGet;
        }

        /// <summary>
        /// Updates and verifies a Nova Process
        /// </summary>
        /// <param name="novaProcessToVerify">The Nova process to verify</param>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="user">The user that updates the Nova process</param>
        /// <returns> The Nova process returned from GetNovaProcess </returns>
        public static NovaProcess UpdateAndVerifyNovaProcess(NovaProcess novaProcessToVerify, IStoryteller storyteller, IUser user)
        {
            ThrowIf.ArgumentNull(novaProcessToVerify, nameof(novaProcessToVerify));
            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Update the process using UpdateNovaProcess
            storyteller.UpdateNovaProcess(user, novaProcessToVerify);

            // Get the process using GetNovaProcess
            var novaProcessReturnedFromGet = storyteller.GetNovaProcess(user, novaProcessToVerify.Id);

            Assert.IsNotNull(novaProcessReturnedFromGet, "GetNovaProcess() returned a null Nova process.");

            // Assert that the process returned from the GetNovaProcess method is identical to the process sent with the UpdateNovaProcess method
            // Allow negative shape ids in the process being verified
            AssertProcessesAreEqual(novaProcessToVerify.Process, novaProcessReturnedFromGet.Process, allowNegativeShapeIds: true);

            // Assert that the decision branch destination links are in sync during the get operations
            AssertDecisionBranchDestinationLinksAreInsync(novaProcessReturnedFromGet.Process);

            return novaProcessReturnedFromGet;
        }

        /// <summary>
        /// Updates, verifies and publishes the process returned from UpdateProcess and GetProcess
        /// </summary>
        /// <param name="processToVerify">The process to verify</param>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="user">The user that updates the process</param>
        /// <returns> The process returned from Get Process </returns>
        public static IProcess UpdateVerifyAndPublishProcess(IProcess processToVerify, IStoryteller storyteller, IUser user)
        {
            // Update and verify the process
            var processReturnedFromGet = UpdateAndVerifyProcess(processToVerify, storyteller, user);

            // Publish the process artifact so it can be deleted in test teardown
            storyteller.PublishProcess(user, processReturnedFromGet);

            return processReturnedFromGet;
        }

        /// <summary>
        /// Verify process status by checking the status boolean parameters from the process model
        /// </summary>
        /// <param name="retrievedProcess">The process model retrieved from the server side</param>
        /// <param name="processStatusState">The process status state that represents expected status of the returned process</param>
        public static void VerifyProcessStatus(IProcess retrievedProcess, ProcessStatusState processStatusState)
        {
            ThrowIf.ArgumentNull(retrievedProcess, nameof(retrievedProcess));

            ProcessStatus expectedStatus = null;

            switch (processStatusState)
            {
                case ProcessStatusState.NeverPublishedAndUpdated:
                    expectedStatus = new ProcessStatus(
                        isLocked: true, isLockedByMe: true, isDeleted: false,
                        isReadOnly: false, isUnpublished: true,
                        hasEverBeenPublished: false);
                    break;
                case ProcessStatusState.PublishedAndNotLocked:
                    expectedStatus = new ProcessStatus(
                        isLocked: false, isLockedByMe: false, isDeleted: false,
                        isReadOnly: false, isUnpublished: false,
                        hasEverBeenPublished: true);
                    break;
                case ProcessStatusState.PublishedAndLockedByMe:
                    expectedStatus = new ProcessStatus(
                        isLocked: true, isLockedByMe: true, isDeleted: false,
                        isReadOnly: false, isUnpublished: true,
                        hasEverBeenPublished: true);
                    break;
                case ProcessStatusState.PublishedAndLockedByAnotherUser:
                    expectedStatus = new ProcessStatus(
                        isLocked: true, isLockedByMe: false, isDeleted: false,
                        isReadOnly: true, isUnpublished: false,
                        hasEverBeenPublished: true);
                    break;
                default:
                    throw new System.ComponentModel.InvalidEnumArgumentException(
                        "ProcessStatus contains invalid enum argument.");
            }

            var retrievedProcessStatus = retrievedProcess.Status;

            Assert.That(retrievedProcessStatus.IsLocked.Equals(expectedStatus.IsLocked),
                "{0} from the process model is {1} but {2} is expected.",
                nameof(retrievedProcessStatus.IsLocked), retrievedProcessStatus.IsLocked, expectedStatus.IsLocked);

            Assert.That(retrievedProcessStatus.IsLockedByMe.Equals(expectedStatus.IsLockedByMe),
                "{0} from the process model is {1} but {2} is expected.",
                nameof(retrievedProcessStatus.IsLockedByMe), retrievedProcessStatus.IsLockedByMe, expectedStatus.IsLockedByMe);

            Assert.That(retrievedProcessStatus.IsDeleted.Equals(expectedStatus.IsDeleted),
                "{0} from the process model is {1} but {2} is expected.",
                nameof(retrievedProcessStatus.IsDeleted), retrievedProcessStatus.IsDeleted, expectedStatus.IsDeleted);

            Assert.That(retrievedProcessStatus.IsReadOnly.Equals(expectedStatus.IsReadOnly),
                "{0} from the process model is {1} but {2} is expected.",
                nameof(retrievedProcessStatus.IsReadOnly), retrievedProcessStatus.IsReadOnly, expectedStatus.IsReadOnly);

            Assert.That(retrievedProcessStatus.IsUnpublished.Equals(expectedStatus.IsUnpublished),
                "{0} from the process model is {1} but {2} is expected.",
                nameof(retrievedProcessStatus.IsUnpublished), retrievedProcessStatus.IsUnpublished, expectedStatus.IsUnpublished);

            Assert.That(retrievedProcessStatus.HasEverBeenPublished.Equals(expectedStatus.HasEverBeenPublished),
                "{0} from the process model is {1} but {2} is expected.",
                nameof(retrievedProcessStatus.HasEverBeenPublished), retrievedProcessStatus.HasEverBeenPublished,
                expectedStatus.HasEverBeenPublished);
        }

        /// <summary>
        /// Create and Get the Default Process
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <returns>The created process</returns>
        public static IProcess CreateAndGetDefaultProcess(IStoryteller storyteller, IProject project, IUser user)
        {
            /*
                [S]--[P]--+--[UT1]--+--[ST2]--+--[E]
            */

            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create default process artifact
            var addedProcessArtifact = storyteller.CreateAndSaveProcessArtifact(project, user);

            // Get default process
            var process = storyteller.GetProcess(user, addedProcessArtifact.Id);

            Assert.IsNotNull(process, "The process returned from GetProcess() was null.");

            return process;
        }

        /// <summary>
        /// Create and Get the Default Nova Process
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the Nova process artifact is created</param>
        /// <param name="user">The user creating the Nova process artifact</param>
        /// <returns>The created Nova process</returns>
        public static NovaProcess CreateAndGetDefaultNovaProcess(IStoryteller storyteller, IProject project, IUser user)
        {
            /*
                [S]--[P]--+--[UT1]--+--[ST2]--+--[E]
            */

            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create default Nova process artifact
            var addedProcessArtifact = storyteller.CreateAndSaveNovaProcessArtifact(project, user);

            // Get default Nova process
            var novaProcess = storyteller.GetNovaProcess(user, addedProcessArtifact.Id);

            Assert.IsNotNull(novaProcess, "The Nova process returned from GetNovaProcess() was null.");

            return novaProcess;
        }

        /// <summary>
        /// Create and Get the Default Process With One Added User Decision
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created process</returns>
        public static IProcess CreateAndGetDefaultProcessWithOneUserDecision(IStoryteller storyteller, IProject project, IUser user, bool updateProcess = true)
        {
            /*
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+--[E]
                              |                        |
                              +-------[UT3]--+--[ST4]--+
            */

            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process
            var process = CreateAndGetDefaultProcess(storyteller, project, user);

            // Find precondition
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the outgoing link for the precondition
            var outgoingLinkForPrecondition = process.GetOutgoingLinkForShape(precondition);

            // Get the branch end point
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch to end
            process.AddUserDecisionPointWithBranchAfterShape(precondition, outgoingLinkForPrecondition.Orderindex + 1, endShape.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            return updateProcess ? storyteller.UpdateProcess(user, process) : process;
        }

        /// <summary>
        /// Create and Get the Default Process With One Added System Decision
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created process</returns>
        public static IProcess CreateAndGetDefaultProcessWithOneSystemDecision(IStoryteller storyteller, IProject project, IUser user, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--[UT1]--+--<SD1>--+--[ST1]--+--[E]
                                     |              |
                                     +----+--[ST2]--+
            */

            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process 
            var process = CreateAndGetDefaultProcess(storyteller, project, user);

            // Find the first UserTask
            var firstUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = process.GetNextShape(firstUserTask);

            // Find the branch end point for system decision points
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing link for the first user task
            var outgoingLinkForFirstUserTask = process.GetOutgoingLinkForShape(firstUserTask);

            // Add System Decision point with branch merging to branchEndPoint
            process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask, outgoingLinkForFirstUserTask.Orderindex + 1, endShape.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            return updateProcess ? storyteller.UpdateProcess(user, process) : process;
        }

        /// <summary>
        /// Create and Get the Default Process With a System Decision that contains Multiple Condition Branches
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="additionalBranches">The number of additional branches after first two branches for the system decision (main branch and first additional branch created with the system decision)</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created process</returns>
        public static IProcess CreateAndGetDefaultProcessWithOneSystemDecisionContainingMultipleConditions(IStoryteller storyteller, IProject project, IUser user, int additionalBranches, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--[E]
                                  |              |
                                  +----+--[ST2]--+
                                  |              |
                                  +----+--[ST3]--+    <--- additionalBranches: 1
            */

            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneSystemDecision(storyteller, project, user,
                updateProcess: false);

            // Find the first UserTask
            var firstUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the outgoing link for the first user task
            var outgoingLinkForFirstUserTask = process.GetOutgoingLinkForShape(firstUserTask);

            // branchOrderIndex for existing system decision
            var defaultAdditionalBranchOrderIndex = outgoingLinkForFirstUserTask.Orderindex + 2;

            // Find the System Decision point with branch merging to branchEndPoint
            var systemDecision = process.GetProcessShapeById(outgoingLinkForFirstUserTask.DestinationId);

            // Find the branch end point for system decision points
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            for (int i = 0; i < additionalBranches; i++)
            {
                // Add branch to the existing System Decision
                process.AddBranchWithSystemTaskToSystemDecisionPoint(systemDecision, defaultAdditionalBranchOrderIndex + i, endShape.Id);
            }

            return updateProcess ? storyteller.UpdateProcess(user, process) : process;
        }

        /// <summary>
        /// Create and Get the Default Process With Inner and Outer System Decisions
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created process</returns>
        public static IProcess CreateAndGetDefaultProcessWithInnerAndOuterSystemDecisions(IStoryteller storyteller, IProject project, IUser user, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--[UT1]--<SD2>--+--<SD1>--+--[ST1]--+--[E]
                                  |         |              |
                                  |         +----+--[ST2]--+
                                  |                        |
                                  +----+--[ST3]--+---------+
            */

            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneSystemDecision(storyteller, project, user,
                updateProcess: false);

            // Find the first UserTask
            var firstUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the branch end point for system decision points
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing link for the first user task
            var outgoingLinkForFirstUserTask = process.GetOutgoingLinkForShape(firstUserTask);

            // Find the system decision before the defaut system task
            var innerSystemDecision = process.GetProcessShapeById(outgoingLinkForFirstUserTask.DestinationId);

            // Add the system decision before the added system decision
            process.AddSystemDecisionPointWithBranchBeforeSystemTask(innerSystemDecision,
                outgoingLinkForFirstUserTask.Orderindex + 1, endShape.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            return updateProcess ? storyteller.UpdateProcess(user, process) : process;
        }


        /// <summary>
        /// Create and Get the Default Process With a System Decision which contains another System Decision on the Second Branch
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created process</returns>
        public static IProcess CreateAndGetDefaultProcessWithSystemDecisionContainingSystemDecisionOnBranch(IStoryteller storyteller, IProject project, IUser user, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]---------+--[E]
                                  |                     |
                                  +----<SD2>--+--[ST2]--+
                                         |              |
                                         +----+--[ST3]--+
            */

            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process with a system decision
            var process = CreateAndGetDefaultProcessWithOneSystemDecision(storyteller, project, user,
                updateProcess: false);

            // Find the first UserTask
            var firstUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the branch end point for system decision points
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing link for the first user task
            var outgoingLinkForFirstUserTask = process.GetOutgoingLinkForShape(firstUserTask);

            // Get the link between the system decision point and the System task on the second branch
            var secondBranchLink = process.Links.Find(l => l.Orderindex.Equals(outgoingLinkForFirstUserTask.Orderindex + 1));

            // Get the system task on the second branch for adding the additional System Decision Point
            var systemTaskFromSecondBranch = process.GetProcessShapeById(secondBranchLink.DestinationId);

            // Add the system decision before the system task on the second branch
            process.AddSystemDecisionPointWithBranchBeforeSystemTask(systemTaskFromSecondBranch,
                outgoingLinkForFirstUserTask.Orderindex + 1, endShape.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            return updateProcess ? storyteller.UpdateProcess(user, process) : process;
        }

        /// <summary>
        /// Create and Get the Default Process With Two Sequential User Tasks and One System Decision the second branch merges
        /// before the second user task
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created process</returns>
        public static IProcess CreateAndGetDefaultProcessWithTwoSequentialUserTasksAndOneSystemDecision(IStoryteller storyteller,
            IProject project, IUser user, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--+--[UT2]--+--[ST2]--+--[E]
                                  |              |
                                  +----+--[ST3]--+
            */
            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));
            // Create a process with two sequential user tasks
            var process = CreateAndGetDefaultProcessWithTwoSequentialUserTasks(storyteller, project, user,
                updateProcess: false);

            // Find the first user task
            var firstUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the first system task following the first user task
            var firstSystemTask = process.GetNextShape(firstUserTask);

            // Find the second user task following the first system task
            var secondUserTask = process.GetNextShape(firstSystemTask);

            // Find the outgoing link for the first user task
            var outgoingLinkForFirstUserTask = process.GetOutgoingLinkForShape(firstUserTask);

            // Add a System Decision with a branch before the first system task
            process.AddSystemDecisionPointWithBranchBeforeSystemTask(firstSystemTask,
                outgoingLinkForFirstUserTask.Orderindex + 1, secondUserTask.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            return updateProcess ? storyteller.UpdateProcess(user, process) : process;
        }

        /// <summary>
        /// Create and Get the Default Process With Two Sequential User Tasks and One System Decision that contains Multiple Condition Branches
        /// merge before the second user task
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="additionalBranches">The number of additional branches after first two branches for the system decision (main branch and first additional branch created with the system decision)</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created process</returns>
        public static IProcess CreateAndGetDefaultProcessWithTwoSequentialUserTasksAndOneSystemDecisionContainingMultipleConditions(IStoryteller storyteller,
            IProject project, IUser user, int additionalBranches, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--+--[UT2]--+--[ST2]--+--[E]
                                  |              |
                                  +----+--[ST3]--+
                                  |              |
                                  +----+--[ST4]--+    <--- additionalBranches: 1
            */
            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));
            // Create a process with two sequential user tasks and a system decision
            var process = CreateAndGetDefaultProcessWithTwoSequentialUserTasksAndOneSystemDecision(storyteller, project, user,
                updateProcess: false);

            // Find the first user task
            var firstUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the outgoing link for the first user task
            var outgoingLinkForFirstUserTask = process.GetOutgoingLinkForShape(firstUserTask);

            // Find the System Decision
            var systemDecision = process.GetNextShape(firstUserTask);

            // Find the first system task following the first user task
            var firstSystemTask = process.GetProcessShapeById(process.Links.Find(l => l.SourceId.Equals(systemDecision.Id) && l.Orderindex.Equals(outgoingLinkForFirstUserTask.Orderindex)).DestinationId);

            // Find the second user task following the first system task
            var secondUserTask = process.GetNextShape(firstSystemTask);

            // branchOrderIndex for existing system decision
            var defaultAdditionalBranchOrderIndex = outgoingLinkForFirstUserTask.Orderindex + 2;

            for (int i = 0; i < additionalBranches; i++)
            {
                // Add branch to the existing System Decision
                process.AddBranchWithSystemTaskToSystemDecisionPoint(systemDecision, defaultAdditionalBranchOrderIndex + i, secondUserTask.Id);
            }

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            return updateProcess ? storyteller.UpdateProcess(user, process) : process;
        }


        /// <summary>
        /// Create and Get the Default Process With a User Decision that contains Multiple Condition Branches
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="additionalBranches">The number of additional branches after first two branches for the user decision (main branch and first additional branch created with the user decision)</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created process</returns>
        public static IProcess CreateAndGetDefaultProcessWithOneUserDecisionContainingMultipleConditions(IStoryteller storyteller, IProject project, IUser user, int additionalBranches, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--[E]
                           |                        |
                           +--[UT3]--+--[ST4]-------+
                           |                        |
                           +--[UT5]--+--[ST6]-------+ <--- additionalBranches: 1
            */

            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process with one user decision
            var process = CreateAndGetDefaultProcessWithOneUserDecision(storyteller, project, user,
                updateProcess: false);

            // Find the precondition
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the outgoing link for the precondition
            var outgoingLinkForPrecondition = process.GetOutgoingLinkForShape(precondition);

            // branchOrderIndex for existing system decision
            var defaultAdditionalBranchOrderIndex = outgoingLinkForPrecondition.Orderindex + 2;

            // Find the User Decision point with branch merging to branchEndPoint
            var userDecision = process.GetNextShape(precondition);

            // Find the branch end point for system decision points
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            for (int i = 0; i < additionalBranches; i++)
            {
                // Add branch to the existing User Decision
                process.AddBranchWithUserAndSystemTaskToUserDecisionPoint(userDecision, defaultAdditionalBranchOrderIndex + i, endShape.Id);
            }

            return updateProcess ? storyteller.UpdateProcess(user, process) : process;
        }

        /// <summary>
        /// Create and Get the Default Process With Two Sequential User Tasks and One User Decision the second branch merge
        /// before the second user task
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created process</returns>
        public static IProcess CreateAndGetDefaultProcessWithTwoSequentialUserTasksAndOneUserDecision(IStoryteller storyteller,
            IProject project, IUser user, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--+--[UT3]--+--[ST4]--+--[E]
                           |                        |
                           +-------[UT5]--+--[ST6]--+

            */
            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));
            // Create a process with two sequential user tasks
            var process = CreateAndGetDefaultProcessWithTwoSequentialUserTasks(storyteller, project, user,
                updateProcess: false);

            // Find the first user task
            var firstUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the first system task following the first user task
            var firstSystemTask = process.GetNextShape(firstUserTask);

            // Find the second user task following the first system task
            var secondUserTask = process.GetNextShape(firstSystemTask);

            // Find the outgoing link for the first user task
            var outgoingLinkForFirstUserTask = process.GetOutgoingLinkForShape(firstUserTask);

            // Add a User Decision with a branch before the first user task
            process.AddUserDecisionPointWithBranchBeforeShape(firstUserTask,
                outgoingLinkForFirstUserTask.Orderindex + 1, secondUserTask.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            return updateProcess ? storyteller.UpdateProcess(user, process) : process;
        }

        /// <summary>
        /// Create and Get the Default Process With Two Sequential User Tasks and One User Decision that contains Multiple Condition Branches
        /// merge before the second user task
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="additionalBranches">The number of additional branches after first two branches for the user decision (main branch and first additional branch created with the user decision)</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created process</returns>
        public static IProcess CreateAndGetDefaultProcessWithTwoSequentialUserTasksAndOneUserDecisionContainingMultipleConditions(IStoryteller storyteller,
            IProject project, IUser user, int additionalBranches, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--+--[UT3]--+--[ST4]--+--[E]
                           |                        |
                           +-------[UT5]--+--[ST6]--+
                           |                        |
                           +-------[UT7]--+--[ST8]--+   <--- additionalBranches: 1
            */
            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));
            // Create a process with two sequential user tasks and one user decision
            var process = CreateAndGetDefaultProcessWithTwoSequentialUserTasksAndOneUserDecision(storyteller, project, user,
                updateProcess: false);

            // Find the precondition
            var preconditon = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the outgoing link for the precondition
            var outgoingLinkForPrecondition = process.GetOutgoingLinkForShape(preconditon);

            // Find the User Decision
            var userDecision = process.GetNextShape(preconditon);

            // Find the User Task From the main branch
            var userTaskFromMainBranch =
                process.GetProcessShapeById(
                    process.Links.Find(
                        l =>
                            l.SourceId.Equals(userDecision.Id) &&
                            l.Orderindex.Equals(outgoingLinkForPrecondition.Orderindex)).DestinationId);

            // Find the first system task following the first user task from the main branch
            var firstSystemTask = process.GetNextShape(userTaskFromMainBranch);

            // Find the second user task following the first system task
            var secondUserTask = process.GetNextShape(firstSystemTask);

            // branchOrderIndex for existing system decision
            var defaultAdditionalBranchOrderIndex = outgoingLinkForPrecondition.Orderindex + 2;

            for (int i = 0; i < additionalBranches; i++)
            {
                // Add branch to the existing User Decision
                process.AddBranchWithUserAndSystemTaskToUserDecisionPoint(userDecision, defaultAdditionalBranchOrderIndex + i, secondUserTask.Id);
            }

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            return updateProcess ? storyteller.UpdateProcess(user, process) : process;
        }

        /// <summary>
        /// Create and Get the Default Process With Two Sequential User Decisions Added
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created process</returns>
        public static IProcess CreateAndGetDefaultProcessWithTwoSequentialUserDecisions(IStoryteller storyteller, IProject project, IUser user, bool updateProcess = true)
        {
            /*
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--<UD2>--+--[UT3]--+--[ST4]--+--[E]
                               |                        |    |                        |
                               +-------[UT5]--+--[ST6]--+    +-------[UT7]--+--[ST8]--+
            */
            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process with one user decision
            var process = CreateAndGetDefaultProcessWithOneUserDecision(storyteller, project, user, updateProcess: false);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Determine the branch endpoint
            var branchEndPoint = process.GetNextShape(preconditionTask);

            // Add user and system task before existing user decision
            process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Add Decision point with branch after precondition
            process.AddUserDecisionPointWithBranchAfterShape(
                preconditionTask,
                preconditionOutgoingLink.Orderindex + 1,
                branchEndPoint.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            return updateProcess ? storyteller.UpdateProcess(user, process) : process;
        }

        /// <summary>
        /// Create and Get the Default Process With Two Sequential System Decisions Added
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created process</returns>
        public static IProcess CreateAndGetDefaultProcessWithTwoSequentialSystemDecisions(IStoryteller storyteller, IProject project, IUser user, bool updateProcess = true)
        {
            /*
                [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--[UT2]--<SD2>--+--[ST3]--+--[E]
                                      |              |           |              |  
                                      +----+--[ST2]--+           +----+--[ST4]--+
            */
            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process with one system decision
            var process = CreateAndGetDefaultProcessWithOneSystemDecision(storyteller, project, user, updateProcess: false);

            // Find the End shape
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            // Add Additional user task along with associated system task
            var addedUserTask = process.AddUserAndSystemTask(process.GetIncomingLinkForShape(endShape));

            // Find the first UserTask
            var firstUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the outgoing link for the first user task
            var outgoingLinkForFirstUserTask = process.GetOutgoingLinkForShape(firstUserTask);

            // Find the link between first system decision to the second branch
            var secondBranchLinkFromFirstSystemDecision = process.Links.Find(
                l => l.Orderindex.Equals(outgoingLinkForFirstUserTask.Orderindex + 1)
                );

            // Find system task on the second branch of the first system decision
            var addedSystemTaskFromSecondBranch =
                process.GetProcessShapeById(secondBranchLinkFromFirstSystemDecision.DestinationId);

            // Update first merging point so that first loop ends before the added user task
            process.GetOutgoingLinkForShape(addedSystemTaskFromSecondBranch).DestinationId = addedUserTask.Id;

            // Find the second system task on the main branch added with additonal user task 
            var secondSystemTaskFromMainBranch = process.GetNextShape(addedUserTask);

            // Add the second System Decision with branch merging to addedUserTask
            process.AddSystemDecisionPointWithBranchBeforeSystemTask(secondSystemTaskFromMainBranch, outgoingLinkForFirstUserTask.Orderindex + 1, endShape.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            return updateProcess ? storyteller.UpdateProcess(user, process) : process;
        }

        /// <summary>
        /// Create and get the default Process with both a User Decision and System Decision added.
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created process</returns>
        public static IProcess CreateAndGetDefaultProcessWithUserAndSystemDecisions(IStoryteller storyteller, IProject project, IUser user, bool updateProcess = true)
        {
            /*
                [S]--[P]--+--<UD1>--+--[UT]---+--[ST]---+--[UT4]--<SD1>--+--[ST5]--+--[E]
                               |                        |           |              |
                               +-------[UT2]--+--[ST3]--+           +----+--[ST7]--+
            */
            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process with one user decision.
            var process = CreateAndGetDefaultProcessWithOneUserDecision(storyteller, project, user, updateProcess: false);

            // Find the End shape.
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            // Add additional user & system task before the End Shape (UT4 & ST5).
            var addedUserTask = process.AddUserAndSystemTask(process.GetIncomingLinkForShape(endShape));

            // Find the incoming link for the last user task.
            var incomingLinkForLastUserTask = process.GetIncomingLinkForShape(addedUserTask);

            // Find the link between user decision to the second branch.
            var secondBranchLinkFromUserDecision = process.Links.Find(
                l => l.Orderindex.Equals(incomingLinkForLastUserTask.Orderindex + 1)
                );

            // Find user & system tasks on the second branch of the user decision.
            var secondLevelUserTask = process.GetProcessShapeById(secondBranchLinkFromUserDecision.DestinationId);
            var secondLevelSystemTask = process.GetNextShape(secondLevelUserTask);

            // Update first merging point so that first loop ends before the added user task.
            process.GetOutgoingLinkForShape(secondLevelSystemTask).DestinationId = addedUserTask.Id;

            // Find the second system task on the main branch added with additonal user task.
            var addedSystemTask = process.GetNextShape(addedUserTask);
            var incomingLinkForAddedSystemTask = process.GetIncomingLinkForShape(addedSystemTask);

            // Add a System Decision with branch merging to addedUserTask (SD1 & ST7).
            process.AddSystemDecisionPointWithBranchBeforeSystemTask(addedSystemTask, incomingLinkForAddedSystemTask.Orderindex + 1, endShape.Id);
            
            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            return updateProcess ? storyteller.UpdateProcess(user, process) : process;
        }

        /// <summary>
        /// Create and Get the Default Process With Two Sequential User Tasks Added
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created process</returns>
        public static IProcess CreateAndGetDefaultProcessWithTwoSequentialUserTasks(IStoryteller storyteller, IProject project, IUser user, bool updateProcess = true)
        {
            /*
                [S]--[P]--+--[UT1]--+--[ST2]--+--[UT3]--+--[ST4]--+--[E]
            */

            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process
            var process = CreateAndGetDefaultProcess(storyteller, project, user);

            // Get the end point
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            // Find outgoing process link for precondition
            var endPointIncomingLink = process.GetIncomingLinkForShape(endShape);

            process.AddUserAndSystemTask(endPointIncomingLink);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            return updateProcess ? storyteller.UpdateProcess(user, process) : process;
        }

        /// <summary>
        /// Creates process with a specified number of pairs of user/system tasks.
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="pairs">The number of pairs to create</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created process</returns>
        public static IProcess CreateProcessWithXAdditionalTaskPairs(
            IStoryteller storyteller, 
            IProject project,
            IUser user, 
            int pairs, 
            bool updateProcess = true)
        {
            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process
            var process = CreateAndGetDefaultProcess(storyteller, project, user);

            // Get the end point
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            // Find incoming process link for the end shape to get the shape
            var endPointIncomingLink = process.GetIncomingLinkForShape(endShape);
            var shapeBeforeEnd = process.GetProcessShapeById(endPointIncomingLink.SourceId);

            // Adds x number of user tasks/system tasks pairs
            process.AddXUserTaskAndSystemTask(shapeBeforeEnd, pairs);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            return updateProcess ? storyteller.UpdateProcess(user, process) : process;
        }

        /// <summary>
        /// Find a Property in an enumeration of Properties
        /// </summary>
        /// <param name="keyToFind">The property to find</param>
        /// <param name="propertiesToSearchThrough">The properties to search though</param>
        /// <returns>The found Property</returns>
        public static KeyValuePair<string, PropertyValueInformation> FindPropertyValue(string keyToFind,
        Dictionary<string, PropertyValueInformation> propertiesToSearchThrough)
        {
            var propertyFound = propertiesToSearchThrough.ToList().Find(p => string.Equals(p.Key, keyToFind, StringComparison.CurrentCultureIgnoreCase));

            Assert.IsNotNull(propertyFound, "Could not find a Property with Name: {0}", keyToFind);

            return propertyFound;
        }

        /// <summary>
        /// Assert that Artifact References are equal
        /// </summary>
        /// <param name="artifactReference1">The first artifact reference</param>
        /// <param name="artifactReference2">The artifact reference being compared to the first</param>
        /// <param name="doDeepCompare">If false, only compare Ids, else compare all properties</param>
        public static void AssertArtifactReferencesAreEqual(ArtifactReference artifactReference1, ArtifactReference artifactReference2, bool doDeepCompare = true)
        {
            if ((artifactReference1 == null) || (artifactReference2 == null))
            {
                Assert.That((artifactReference1 == null) && (artifactReference2 == null), "One of the artifact references is null while the other is not null");
            }
            else
            {
                Assert.AreEqual(artifactReference1.Id, artifactReference2.Id, "Artifact references ids do not match");

                if (doDeepCompare)
                {
                    Assert.AreEqual(artifactReference1.BaseItemTypePredefined, artifactReference2.BaseItemTypePredefined,
                        "Artifact reference base item types do not match");
                    Assert.AreEqual(artifactReference1.Link, artifactReference2.Link, "Artifact reference links do not match");
                    Assert.AreEqual(artifactReference1.Name, artifactReference2.Name, "Artifact reference names do not match");
                    Assert.AreEqual(artifactReference1.ProjectId, artifactReference2.ProjectId, "Artifact reference project ids do not match");
                    Assert.AreEqual(artifactReference1.TypePrefix, artifactReference2.TypePrefix, "Artifact reference type prefixes do not match");
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Assert that Property values are equal
        /// </summary>
        /// <param name="propertyValue1">The first Property value</param>
        /// <param name="propertyValue2">The Property value being compared to the first</param>
        private static void AssertPropertyValuesAreEqual(PropertyValueInformation propertyValue1,
            PropertyValueInformation propertyValue2)
        {
            Assert.AreEqual(propertyValue1.PropertyName, propertyValue2.PropertyName, "Property names do not match: {0} != {1}", propertyValue1.PropertyName, propertyValue2.PropertyName);
            Assert.AreEqual(propertyValue1.TypePredefined, propertyValue2.TypePredefined, "Property types do not match");
            Assert.AreEqual(propertyValue1.TypeId, propertyValue2.TypeId, "Property type ids do not match");

            // Asserts story links only if not null
            if (propertyValue1.PropertyName == "StoryLinks" && propertyValue1.Value != null)
            {
                AssertStoryLinksAreEqual((StoryLink)propertyValue1.Value, (StoryLink)propertyValue2.Value);
            }
        }

        /// <summary>
        /// Assert that Process Links are equal
        /// </summary>
        /// <param name="process1">The first process containing the links to compare</param>
        /// <param name="process2">The process containing the links being compared to the first</param>
        private static void AssertLinksAreEqual(IProcess process1, IProcess process2)
        {
            foreach (var link1 in process1.Links)
            {
                var link2 = new ProcessLink();

                if (link1.SourceId > 0 && link1.DestinationId > 0)
                {
                    link2 = FindProcessLink(link1, process2.Links);
                }
                // If the destination id is < 0, we find the name of the destination shape and 
                // then locate this shape in the second process. We then replace the destination id
                // of the first link with the id of that shape.  This allows us to compare links.
                else if (link1.SourceId > 0 && link1.DestinationId < 0)
                {
                    var link1DestinationShape = FindProcessShapeById(link1.DestinationId, process1.Shapes);
                    var link2Shape = FindProcessShapeByName(link1DestinationShape.Name, process2.Shapes);

                    link1.DestinationId = link2Shape.Id;

                    link2 = FindProcessLink(link1, process2.Links);
                }
                // If the source id is < 0, we find the name of the source shape and 
                // then locate this shape in the second process. We then replace the source id
                // of the first link with the id of that shape.  This allows us to compare links.
                else if (link1.SourceId < 0 && link1.DestinationId > 0)
                {
                    var link1SourceShape = FindProcessShapeById(link1.SourceId, process1.Shapes);
                    var link2Shape = FindProcessShapeByName(link1SourceShape.Name, process2.Shapes);

                    link1.SourceId = link2Shape.Id;

                    link2 = FindProcessLink(link1, process2.Links);
                }
                else if (link1.SourceId < 0 && link1.DestinationId < 0)
                {
                    var link1SourceShape = FindProcessShapeById(link1.SourceId, process1.Shapes);
                    var link2Shape = FindProcessShapeByName(link1SourceShape.Name, process2.Shapes);

                    link1.SourceId = link2Shape.Id;

                    var link1DestinationShape = FindProcessShapeById(link1.DestinationId, process1.Shapes);
                    link2Shape = FindProcessShapeByName(link1DestinationShape.Name, process2.Shapes);

                    link1.DestinationId = link2Shape.Id;

                    link2 = FindProcessLink(link1, process2.Links);
                }

                Assert.AreEqual(link1.SourceId, link2.SourceId, "Link sources do not match");
                Assert.AreEqual(link1.DestinationId, link2.DestinationId, "Link destinations do not match");
                Assert.AreEqual(link1.Label, link2.Label, "Link labels do not match");
                Assert.AreEqual(link1.Orderindex, link2.Orderindex, "Link order indexes do not match");
            }
        }

        /// <summary>
        /// Assert that Process Shapes are equal
        /// </summary>
        /// <param name="shape1">The first Shape</param>
        /// <param name="shape2">The Shape being compared to the first</param>
        /// <param name="allowNegativeShapeIds">Allows for inequality of shape ids where a newly added shape has a negative id</param>
        /// <param name="isCopiedProcess">(optional) Flag indicating if the process being compared is a copied process</param>
        private static void AssertShapesAreEqual(IProcessShape shape1, IProcessShape shape2, bool allowNegativeShapeIds, bool isCopiedProcess)
        {
            // Do not perform Id comparisons for copied processes
            if (!isCopiedProcess)
            {
                // Note that if a shape id of the first Process being compared is less than 0, then the first Process 
                // is a process that will be updated with proper id values at the back end.  If the shape id of
                // the first process being compared is greater than 0, then the shape ids should match.
                if (allowNegativeShapeIds && shape1.Id < 0)
                {
                    Assert.That(shape2.Id > 0, "Returned shape id was negative");
                }
                else if (allowNegativeShapeIds && shape2.Id < 0)
                {
                    Assert.That(shape1.Id > 0, "Returned shape id was negative");
                }
                else
                {
                    Assert.AreEqual(shape1.Id, shape2.Id, "Shape ids do not match");
                }
            }

            Assert.AreEqual(shape1.Name, shape2.Name, "Shape names do not match");
            Assert.AreEqual(shape1.BaseItemTypePredefined, shape2.BaseItemTypePredefined,
                "Shape base item types do not match");

            // Project and parent ids are not necessarily the same for copied processes
            if (!isCopiedProcess)
            {
                Assert.AreEqual(shape1.ProjectId, shape2.ProjectId, "Shape project ids do not match");
                Assert.AreEqual(shape1.ParentId, shape2.ParentId, "Shape parent ids do not match");
            }

            Assert.AreEqual(shape1.TypePrefix, shape2.TypePrefix, "Shape type prefixes do not match");

            // Assert associated artifacts are equal by checking artifact Id only
            AssertArtifactReferencesAreEqual(shape1.AssociatedArtifact, shape2.AssociatedArtifact, doDeepCompare: false);

            // Assert persona references are equal by checking artifact Id only
            AssertArtifactReferencesAreEqual(shape1.PersonaReference, shape2.PersonaReference, doDeepCompare: false);

            // Assert that Shape properties are equal
            foreach (var shape1Property in shape1.PropertyValues)
            {
                var shape2Property = FindPropertyValue(shape1Property.Key, shape2.PropertyValues);

                AssertPropertyValuesAreEqual(shape1Property.Value, shape2Property.Value);
            }
        }

        /// <summary>
        /// Assert that Story Links are equal
        /// </summary>
        /// <param name="link1">The first Story Link</param>
        /// <param name="link2">The Story Link being compared to the first</param>
        private static void AssertStoryLinksAreEqual(StoryLink link1, StoryLink link2)
        {
            Assert.AreEqual(link1.AssociatedReferenceArtifactId, link2.AssociatedReferenceArtifactId, "Link associated reference artifact ids do not match");
            Assert.AreEqual(link1.DestinationId, link2.DestinationId, "Link destinations do not match");
            Assert.AreEqual(link1.SourceId, link2.SourceId, "Link sources do not match");
            Assert.AreEqual(link1.Orderindex, link2.Orderindex, "Link order indexes do not match");
        }


        /// <summary>
        /// Assert that DecisionBranchDestinationLinks information are up-to-dated with number of branches from the process model
        /// </summary>
        /// <param name="process">The process to be verified for branch merging links</param>
        private static void AssertDecisionBranchDestinationLinksAreInsync(IProcess process)
        {
            // Total number of branches from the process
            var userDecisions = process.GetProcessShapesByShapeType(ProcessShapeType.UserDecision);
            var systemDecisions = process.GetProcessShapesByShapeType(ProcessShapeType.SystemDecision);

            // Adding all branches from available decisions from the process
            int totalNumberOfBranchesFromUserDecision = 
                userDecisions.Sum(ud => process.GetOutgoingLinksForShape(ud).Count() - 1);

            int totalNumberOfBranchesFromSystemDecision =
                systemDecisions.Sum(sd => process.GetOutgoingLinksForShape(sd).Count() - 1);

            int totalNumberOfBranches = totalNumberOfBranchesFromUserDecision + totalNumberOfBranchesFromSystemDecision;

            var decisionBranchDesinationLinkCount = process.DecisionBranchDestinationLinks?.Count ?? 0;

            // Verify that total number of DecisionBranchDestinationLinks equal to total number of branch from the process
            Assert.That(decisionBranchDesinationLinkCount.Equals(totalNumberOfBranches),
                "The total number of branches from the process is {0} but The DecisionBranchDestinationLink contains {1} links.",
                totalNumberOfBranches, decisionBranchDesinationLinkCount);
        }

        /// <summary>
        /// Find a Process Link in an enumeration of Process Links
        /// </summary>
        /// <param name="linkToFind">The process link to find</param>
        /// <param name="linksToSearchThrough">The process links to search</param>
        /// <returns>The found process link</returns>
        private static ProcessLink FindProcessLink(ProcessLink linkToFind,
            List<ProcessLink> linksToSearchThrough)
        {
            var linkFound = linksToSearchThrough.Find(l => l.SourceId == linkToFind.SourceId && l.DestinationId == linkToFind.DestinationId);
 
            Assert.IsNotNull(linkFound, "Could not find and ProcessLink with Source Id {0} and Destination Id {1}", linkToFind.SourceId, linkToFind.DestinationId);

            return linkFound;
        }

        /// <summary>
        /// Find a Process Shape by name in an enumeration of Process Shapes
        /// </summary>
        /// <param name="shapeName">The name of the process shape to find</param>
        /// <param name="shapesToSearchThrough">The process shapes to search</param>
        /// <returns>The found process shape</returns>
        private static IProcessShape FindProcessShapeByName(string shapeName,
            List<ProcessShape> shapesToSearchThrough)
        {
            ThrowIf.ArgumentNull(shapesToSearchThrough, nameof(shapesToSearchThrough));

            var shapeFound = shapesToSearchThrough.Find(s => s.Name == shapeName);

            Assert.IsNotNull(shapeFound, "Could not find a Process Shape with Name {0}", shapeName);

            return shapeFound;
        }

        /// <summary>
        /// Find a Process Shape by id in an enumeration of Process Shapes
        /// </summary>
        /// <param name="shapeId">The id of the process shape to find</param>
        /// <param name="shapesToSearchThrough">The process shapes to search</param>
        /// <returns>The found process shape</returns>
        private static IProcessShape FindProcessShapeById(int shapeId,
            List<ProcessShape> shapesToSearchThrough)
        {
            var shapeFound = shapesToSearchThrough.Find(s => s.Id == shapeId);

            Assert.IsNotNull(shapeFound, "Could not find a Process Shape with Id {0}", shapeId);

            return shapeFound;
        }

        #endregion Private Methods
    }
}
