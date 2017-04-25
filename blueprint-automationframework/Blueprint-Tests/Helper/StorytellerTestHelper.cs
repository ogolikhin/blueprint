using Model;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.ComponentModel;
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
        /// Asserts that a persona reference relationship is correct for a given actor
        /// </summary>
        /// <param name="relationship">The relationship being tested.</param>
        /// <param name="actorArtifactProject">The project where the actor artifact is located.</param>
        /// <param name="actorArtifact">The actor artifact</param>
        /// <param name="isReadOnlyExpected">(optional) Flag indicating whether the relationship is expected to be read-only (Default: false)</param>
        /// <param name="isSuspectExpected">(optional) Flag indicating whether the relationship is expected to be suspect (Default: false)</param>
        public static void AssertPersonaReferenceRelationshipIsCorrect(Relationship relationship, 
            IProject actorArtifactProject, 
            NovaArtifactBase actorArtifact,
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

            var novaProcess = new NovaProcess { Id = processToVerify.Id, ProjectId = processToVerify.ProjectId, Process = (Process)processToVerify };

            var updatedNovaProcess = UpdateAndVerifyNovaProcess(novaProcess, storyteller, user);

            return updatedNovaProcess.Process;
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
            Process.AssertAreEqual(novaProcessToVerify.Process, novaProcessReturnedFromGet.Process, allowNegativeShapeIds: true);

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
            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));

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
                    throw new InvalidEnumArgumentException(
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
            var novaProcess = CreateAndGetDefaultNovaProcess(storyteller, project, user);

            return novaProcess.Process;
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
            var novaProcess = storyteller.CreateAndSaveNovaProcessArtifact(project, user);

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

        #endregion Public Methods

        #region Private Methods

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

        #endregion Private Methods
    }
}
