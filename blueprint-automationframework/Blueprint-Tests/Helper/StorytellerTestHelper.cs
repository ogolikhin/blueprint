using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.ModelHelpers;
using Model.StorytellerModel;
using Model.StorytellerModel.Enums;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.ComponentModel;
using System.Linq;
using Utilities;
using Utilities.Factories;

namespace Helper
{
    public static class StorytellerTestHelper
    {
        public static IArtifactStore ArtifactStore { get; } = ArtifactStoreFactory.GetArtifactStoreFromTestConfig();

        public static IStoryteller Storyteller { get; } = StorytellerFactory.GetStorytellerFromTestConfig();
        public static ISvcShared SvcShared { get; } = SvcSharedFactory.GetSvcSharedFromTestConfig();


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
            INovaArtifactBase actorArtifact,
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
        /// Updates, verifies and publishes the process returned from UpdateProcess and GetProcess
        /// </summary>
        /// <param name="processToVerify">The process to verify</param>
        /// <param name="user">The user that updates the process</param>
        /// <returns> The nova process returned from Get Process </returns>
        public static INovaProcess UpdateVerifyAndPublishNovaProcess(INovaProcess processToVerify, IUser user)
        {
            // Update and verify the process
            var novaProcessReturnedFromGet = UpdateAndVerifyNovaProcess(processToVerify, user);

            // Publish the process artifact so it can be deleted in test teardown
            var response = ArtifactStore.PublishArtifact(novaProcessReturnedFromGet.Id, user);

            novaProcessReturnedFromGet.Version = response.Artifacts.First().Version;

            return novaProcessReturnedFromGet;
        }

        /// <summary>
        /// Updates and verifies a Nova Process
        /// </summary>
        /// <param name="novaProcessToVerify">The Nova process to verify</param>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="user">The user that updates the Nova process</param>
        /// <returns> The Nova process returned from GetNovaProcess </returns>
        public static INovaProcess UpdateAndVerifyNovaProcess(INovaProcess novaProcessToVerify, IUser user)
        {
            ThrowIf.ArgumentNull(novaProcessToVerify, nameof(novaProcessToVerify));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Update the process using UpdateNovaProcess
            ArtifactStore.UpdateNovaProcess(user, novaProcessToVerify);

            // Get the process using GetNovaProcess
            var novaProcessReturnedFromGet = ArtifactStore.GetNovaProcess(user, novaProcessToVerify.Id);

            Assert.IsNotNull(novaProcessReturnedFromGet, "GetNovaProcess() returned a null Nova process.");

            // Assert that the process returned from the GetNovaProcess method is identical to the process sent with the UpdateNovaProcess method
            // Allow negative shape ids in the process being verified
            Process.AssertAreEqual(novaProcessToVerify.Process, novaProcessReturnedFromGet.Process, allowNegativeShapeIds: true);

            // Assert that the decision branch destination links are in sync during the get operations
            AssertDecisionBranchDestinationLinksAreInsync(novaProcessReturnedFromGet.Process);

            return novaProcessReturnedFromGet;
        }

        /// <summary>
        /// Verify process status by checking the status boolean parameters from the process model
        /// </summary>
        /// <param name="processStatusState">The process status state that represents expected status of the returned process</param>
        /// <param name="retrievedProcess">The process model retrieved from the server side</param>
        public static void VerifyProcessStatus(ProcessStatusState processStatusState, IProcess retrievedProcess)
        {
            ThrowIf.ArgumentNull(processStatusState, nameof(processStatusState));
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

            Assert.AreEqual(expectedStatus.IsLocked, retrievedProcessStatus.IsLocked, 
                "{0} from the process model is {0} is expected from {1} but {2} is returned.",
                expectedStatus.IsLocked, nameof(retrievedProcessStatus.IsLocked), retrievedProcessStatus.IsLocked);

            Assert.AreEqual(expectedStatus.IsLockedByMe, retrievedProcessStatus.IsLockedByMe,
                "{0} from the process model is {0} is expected from {1} but {2} is returned.",
                expectedStatus.IsLockedByMe, nameof(retrievedProcessStatus.IsLockedByMe), retrievedProcessStatus.IsLockedByMe);

            Assert.AreEqual(expectedStatus.IsDeleted, retrievedProcessStatus.IsDeleted,
                "{0} from the process model is {0} is expected from {1} but {2} is returned.",
                expectedStatus.IsDeleted, nameof(retrievedProcessStatus.IsDeleted), retrievedProcessStatus.IsDeleted);

            Assert.AreEqual(expectedStatus.IsReadOnly, retrievedProcessStatus.IsReadOnly,
                "{0} from the process model is {0} is expected from {1} but {2} is returned.",
                expectedStatus.IsReadOnly, nameof(retrievedProcessStatus.IsReadOnly), retrievedProcessStatus.IsReadOnly);

            Assert.AreEqual(expectedStatus.IsUnpublished, retrievedProcessStatus.IsUnpublished,
                "{0} from the process model is {0} is expected from {1} but {2} is returned.",
                expectedStatus.IsUnpublished, nameof(retrievedProcessStatus.IsUnpublished), retrievedProcessStatus.IsUnpublished);

            Assert.AreEqual(expectedStatus.HasEverBeenPublished, retrievedProcessStatus.HasEverBeenPublished,
                "{0} from the process model is {0} is expected from {1} but {2} is returned.",
                expectedStatus.HasEverBeenPublished, nameof(retrievedProcessStatus.HasEverBeenPublished), retrievedProcessStatus.HasEverBeenPublished);

        }


        /// <summary>
        /// Creates and saves a new Nova Process artifact (wrapped inside an ProcessArtifactWrapper that tracks the state of the process artifact.).
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="project">The project where the Nova artifact should be created.</param>
        /// <param name="parentId">(optional) The parent ID of this Nova artifact.
        ///     By default the parent should be the project.</param>
        /// <param name="orderIndex">(optional) The order index of this Nova artifact.
        ///     By default the order index should be after the last artifact.</param>
        /// <param name="name">(optional) The artifact name.  By default a random name is created.</param>
        /// <returns>The Nova artifact wrapped in an ProcessArtifactWrapper that tracks the state of the artifact.</returns>
        public static ProcessArtifactWrapper CreateNovaProcessArtifact(
            IUser user, IProject project,
            int? parentId = null, double? orderIndex = null, string name = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            name = name ?? RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            var artifact = Model.Impl.ArtifactStore.CreateArtifact(ArtifactStore.Address, user,
                ItemTypePredefined.Process, name, project, parentId, orderIndex);

            var process = Storyteller.GetNovaProcess(user, artifact.Id);

            return WrapProcessArtifact(process, project, user);
        }

        /// <summary>
        /// Creates and publish a new Nova Process artifact (wrapped inside an ProcessArtifactWrapper that tracks the state of the artifact.).
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="project">The project where the Nova Process artifact should be created.</param>
        /// <param name="parentId">(optional) The parent of this Nova Process artifact.
        ///     By default the parent should be the project.</param>
        /// <param name="orderIndex">(optional) The order index of this Nova Process artifact.
        ///     By default the order index should be after the last artifact.</param>
        /// <param name="name">(optional) The artifact name.  By default a random name is created.</param>
        /// <returns>The Nova Process artifact wrapped in an ProcessArtifactWrapper that tracks the state of the artifact.</returns>
        public static ProcessArtifactWrapper CreateAndPublishNovaProcessArtifact(
            IUser user, IProject project,
            int? parentId = null, double? orderIndex = null, string name = null)
        {
            var wrappedProcessArtifact = CreateNovaProcessArtifact(user, project, parentId, orderIndex, name);
            wrappedProcessArtifact.Publish(user);

            return wrappedProcessArtifact;
        }

        /// <summary>
        /// Wraps an INovaProcess in an ProcessArtifactWrapper and adds it the list of artifacts that get disposed.
        /// </summary>
        /// <param name="novaProcess">The INovaProcess that was created by ArtifactStore.</param>
        /// <param name="project">The project where the artifact was created.</param>
        /// <param name="createdBy">The user that created this artifact.</param>
        /// <returns>The ProcessArtifactWrapper for the novaProcessArtifact.</returns>
        public static ProcessArtifactWrapper WrapProcessArtifact(INovaProcess novaProcess, IProject project, IUser createdBy)
        {
            ThrowIf.ArgumentNull(novaProcess, nameof(novaProcess));

            var wrappedProcessArtifact = new ProcessArtifactWrapper(novaProcess, ArtifactStore, SvcShared, project, createdBy);

            return wrappedProcessArtifact;
        }

        /// <summary>
        /// Create and Get the Default Nova Process
        /// </summary>
        /// <param name="project">The project where the Nova process artifact is created</param>
        /// <param name="user">The user creating the Nova process artifact</param>
        /// <returns>The created Nova process</returns>
        public static ProcessArtifactWrapper CreateAndGetDefaultNovaProcess(IProject project, IUser user)
        {
            /*
                [S]--[P]--+--[UT1]--+--[ST2]--+--[E]
            */

            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create default Nova process artifact
            var novaProcess = CreateNovaProcessArtifact(user, project);

            Assert.IsNotNull(novaProcess, "The Nova process returned from GetNovaProcess() was null.");

            return novaProcess;
        }

        /// <summary>
        /// Create and Get the Default Nova Process With One Added User Decision
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created Nova process</returns>
        public static ProcessArtifactWrapper CreateAndGetDefaultNovaProcessWithOneUserDecision(IProject project, IUser user, bool updateProcess = true)
        {
            /*
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+--[E]
                              |                        |
                              +-------[UT3]--+--[ST4]--+
            */

            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process
            var novaProcess = CreateAndGetDefaultNovaProcess(project, user);

            // Find precondition
            var precondition = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the outgoing link for the precondition
            var outgoingLinkForPrecondition = novaProcess.Process.GetOutgoingLinkForShape(precondition);

            // Get the branch end point
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch to end
            novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(precondition, outgoingLinkForPrecondition.Orderindex + 1, endShape.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            if (updateProcess)
            {
                novaProcess.Update(user, novaProcess.NovaProcess);
                novaProcess.RefreshArtifactFromServer(user);
            }
            return novaProcess;
        }

        /// <summary>
        /// Create and Get the Default Nova Process With One Added System Decision
        /// </summary>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created Nova process</returns>
        public static ProcessArtifactWrapper CreateAndGetDefaultNovaProcessWithOneSystemDecision(IProject project, IUser user, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--[UT1]--+--<SD1>--+--[ST1]--+--[E]
                                     |              |
                                     +----+--[ST2]--+
            */

            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process 
            var novaProcess = CreateAndGetDefaultNovaProcess(project, user);

            // Find the first UserTask
            var firstUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = novaProcess.Process.GetNextShape(firstUserTask);

            // Find the branch end point for system decision points
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing link for the first user task
            var outgoingLinkForFirstUserTask = novaProcess.Process.GetOutgoingLinkForShape(firstUserTask);

            // Add System Decision point with branch merging to branchEndPoint
            novaProcess.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask, outgoingLinkForFirstUserTask.Orderindex + 1, endShape.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            if (updateProcess)
            {
                novaProcess.Update(user, novaProcess.NovaProcess);
                novaProcess.RefreshArtifactFromServer(user);
            }
            return novaProcess;
        }

        /// <summary>
        /// Create and Get the Default Nova Process With a System Decision that contains Multiple Condition Branches
        /// </summary>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="additionalBranches">The number of additional branches after first two branches for the system decision (main branch and first additional branch created with the system decision)</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created Nova process</returns>
        public static ProcessArtifactWrapper CreateAndGetDefaultNovaProcessWithOneSystemDecisionContainingMultipleConditions(IProject project, IUser user, int additionalBranches, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--[E]
                                  |              |
                                  +----+--[ST2]--+
                                  |              |
                                  +----+--[ST3]--+    <--- additionalBranches: 1
            */

            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process
            var novaProcess = CreateAndGetDefaultNovaProcessWithOneSystemDecision(project, user,
                updateProcess: false);

            // Find the first UserTask
            var firstUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the outgoing link for the first user task
            var outgoingLinkForFirstUserTask = novaProcess.Process.GetOutgoingLinkForShape(firstUserTask);

            // branchOrderIndex for existing system decision
            var defaultAdditionalBranchOrderIndex = outgoingLinkForFirstUserTask.Orderindex + 2;

            // Find the System Decision point with branch merging to branchEndPoint
            var systemDecision = novaProcess.Process.GetProcessShapeById(outgoingLinkForFirstUserTask.DestinationId);

            // Find the branch end point for system decision points
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            for (int i = 0; i < additionalBranches; i++)
            {
                // Add branch to the existing System Decision
                novaProcess.Process.AddBranchWithSystemTaskToSystemDecisionPoint(systemDecision, defaultAdditionalBranchOrderIndex + i, endShape.Id);
            }

            if (updateProcess)
            {
                novaProcess.Update(user, novaProcess.NovaProcess);
                novaProcess.RefreshArtifactFromServer(user);
            }
            return novaProcess;
        }

        /// <summary>
        /// Create and Get the Default Nova Process With Inner and Outer System Decisions
        /// </summary>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created Nova process</returns>
        public static ProcessArtifactWrapper CreateAndGetDefaultNovaProcessWithInnerAndOuterSystemDecisions(IProject project, IUser user, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--[UT1]--<SD2>--+--<SD1>--+--[ST1]--+--[E]
                                  |         |              |
                                  |         +----+--[ST2]--+
                                  |                        |
                                  +----+--[ST3]--+---------+
            */

            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process
            var novaProcess = CreateAndGetDefaultNovaProcessWithOneSystemDecision(project, user,
                updateProcess: false);

            // Find the first UserTask
            var firstUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the branch end point for system decision points
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing link for the first user task
            var outgoingLinkForFirstUserTask = novaProcess.Process.GetOutgoingLinkForShape(firstUserTask);

            // Find the system decision before the defaut system task
            var innerSystemDecision = novaProcess.Process.GetProcessShapeById(outgoingLinkForFirstUserTask.DestinationId);

            // Add the system decision before the added system decision
            novaProcess.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(innerSystemDecision,
                outgoingLinkForFirstUserTask.Orderindex + 1, endShape.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            if (updateProcess)
            {
                novaProcess.Update(user, novaProcess.NovaProcess);
                novaProcess.RefreshArtifactFromServer(user);
            }
            return novaProcess;
        }

        /// <summary>
        /// Create and Get the Default Nova Process With a System Decision which contains another System Decision on the Second Branch
        /// </summary>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created Nova process</returns>
        public static ProcessArtifactWrapper CreateAndGetDefaultNovaProcessWithSystemDecisionContainingSystemDecisionOnBranch(IProject project, IUser user, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]---------+--[E]
                                  |                     |
                                  +----<SD2>--+--[ST2]--+
                                         |              |
                                         +----+--[ST3]--+
            */

            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process with a system decision
            var novaProcess = CreateAndGetDefaultNovaProcessWithOneSystemDecision(project, user,
                updateProcess: false);

            // Find the first UserTask
            var firstUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the branch end point for system decision points
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing link for the first user task
            var outgoingLinkForFirstUserTask = novaProcess.Process.GetOutgoingLinkForShape(firstUserTask);

            // Get the link between the system decision point and the System task on the second branch
            var secondBranchLink = novaProcess.Process.Links.Find(l => l.Orderindex.Equals(outgoingLinkForFirstUserTask.Orderindex + 1));

            // Get the system task on the second branch for adding the additional System Decision Point
            var systemTaskFromSecondBranch = novaProcess.Process.GetProcessShapeById(secondBranchLink.DestinationId);

            // Add the system decision before the system task on the second branch
            novaProcess.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(systemTaskFromSecondBranch,
                outgoingLinkForFirstUserTask.Orderindex + 1, endShape.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            if (updateProcess)
            {
                novaProcess.Update(user, novaProcess.NovaProcess);
                novaProcess.RefreshArtifactFromServer(user);
            }
            return novaProcess;
        }

        /// <summary>
        /// Create and Get the Default Nova Process With Two Sequential User Tasks and One System Decision the second branch merges
        /// before the second user task
        /// </summary>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created Nova process</returns>
        public static ProcessArtifactWrapper CreateAndGetDefaultNovaProcessWithTwoSequentialUserTasksAndOneSystemDecision(
            IProject project, IUser user, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--+--[UT2]--+--[ST2]--+--[E]
                                  |              |
                                  +----+--[ST3]--+
            */
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));
            // Create a process with two sequential user tasks
            var novaProcess = CreateAndGetDefaultNovaProcessWithTwoSequentialUserTasks(project, user,
                updateProcess: false);

            // Find the first user task
            var firstUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the first system task following the first user task
            var firstSystemTask = novaProcess.Process.GetNextShape(firstUserTask);

            // Find the second user task following the first system task
            var secondUserTask = novaProcess.Process.GetNextShape(firstSystemTask);

            // Find the outgoing link for the first user task
            var outgoingLinkForFirstUserTask = novaProcess.Process.GetOutgoingLinkForShape(firstUserTask);

            // Add a System Decision with a branch before the first system task
            novaProcess.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(firstSystemTask,
                outgoingLinkForFirstUserTask.Orderindex + 1, secondUserTask.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            if (updateProcess)
            {
                novaProcess.Update(user, novaProcess.NovaProcess);
                novaProcess.RefreshArtifactFromServer(user);
            }
            return novaProcess;
        }

        /// <summary>
        /// Create and Get the Default Nova Process With Two Sequential User Tasks and One System Decision that contains Multiple Condition Branches
        /// merge before the second user task
        /// </summary>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="additionalBranches">The number of additional branches after first two branches for the system decision (main branch and first additional branch created with the system decision)</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created Nova process</returns>
        public static ProcessArtifactWrapper CreateAndGetDefaultNovaProcessWithTwoSequentialUserTasksAndOneSystemDecisionContainingMultipleConditions(
            IProject project, IUser user, int additionalBranches, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--+--[UT2]--+--[ST2]--+--[E]
                                  |              |
                                  +----+--[ST3]--+
                                  |              |
                                  +----+--[ST4]--+    <--- additionalBranches: 1
            */
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));
            // Create a process with two sequential user tasks and a system decision
            var novaProcess = CreateAndGetDefaultNovaProcessWithTwoSequentialUserTasksAndOneSystemDecision(project, user,
                updateProcess: false);

            // Find the first user task
            var firstUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the outgoing link for the first user task
            var outgoingLinkForFirstUserTask = novaProcess.Process.GetOutgoingLinkForShape(firstUserTask);

            // Find the System Decision
            var systemDecision = novaProcess.Process.GetNextShape(firstUserTask);

            // Find the first system task following the first user task
            var firstSystemTask = novaProcess.Process.GetProcessShapeById(novaProcess.Process.Links.Find(l => l.SourceId.Equals(systemDecision.Id) && l.Orderindex.Equals(outgoingLinkForFirstUserTask.Orderindex)).DestinationId);

            // Find the second user task following the first system task
            var secondUserTask = novaProcess.Process.GetNextShape(firstSystemTask);

            // branchOrderIndex for existing system decision
            var defaultAdditionalBranchOrderIndex = outgoingLinkForFirstUserTask.Orderindex + 2;

            for (int i = 0; i < additionalBranches; i++)
            {
                // Add branch to the existing System Decision
                novaProcess.Process.AddBranchWithSystemTaskToSystemDecisionPoint(systemDecision, defaultAdditionalBranchOrderIndex + i, secondUserTask.Id);
            }

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            if (updateProcess)
            {
                novaProcess.Update(user, novaProcess.NovaProcess);
                novaProcess.RefreshArtifactFromServer(user);
            }
            return novaProcess;
        }

        /// <summary>
        /// Create and Get the Default Nova Process With a User Decision that contains Multiple Condition Branches
        /// </summary>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="additionalBranches">The number of additional branches after first two branches for the user decision (main branch and first additional branch created with the user decision)</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created Nova process</returns>
        public static ProcessArtifactWrapper CreateAndGetDefaultNovaProcessWithOneUserDecisionContainingMultipleConditions(IProject project, IUser user, int additionalBranches, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--[E]
                           |                        |
                           +--[UT3]--+--[ST4]-------+
                           |                        |
                           +--[UT5]--+--[ST6]-------+ <--- additionalBranches: 1
            */

            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process with one user decision
            var novaProcess = CreateAndGetDefaultNovaProcessWithOneUserDecision(project, user,
                updateProcess: false);

            // Find the precondition
            var precondition = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the outgoing link for the precondition
            var outgoingLinkForPrecondition = novaProcess.Process.GetOutgoingLinkForShape(precondition);

            // branchOrderIndex for existing system decision
            var defaultAdditionalBranchOrderIndex = outgoingLinkForPrecondition.Orderindex + 2;

            // Find the User Decision point with branch merging to branchEndPoint
            var userDecision = novaProcess.Process.GetNextShape(precondition);

            // Find the branch end point for system decision points
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            for (int i = 0; i < additionalBranches; i++)
            {
                // Add branch to the existing User Decision
                novaProcess.Process.AddBranchWithUserAndSystemTaskToUserDecisionPoint(userDecision, defaultAdditionalBranchOrderIndex + i, endShape.Id);
            }

            if (updateProcess)
            {
                novaProcess.Update(user, novaProcess.NovaProcess);
                novaProcess.RefreshArtifactFromServer(user);
            }
            return novaProcess;
        }

        /// <summary>
        /// Create and Get the Default Nova Process With Two Sequential User Tasks and One User Decision the second branch merge
        /// before the second user task
        /// </summary>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created Nova process</returns>
        public static ProcessArtifactWrapper CreateAndGetDefaultNovaProcessWithTwoSequentialUserTasksAndOneUserDecision(
            IProject project, IUser user, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--+--[UT3]--+--[ST4]--+--[E]
                           |                        |
                           +-------[UT5]--+--[ST6]--+

            */
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));
            // Create a process with two sequential user tasks
            var novaProcess = CreateAndGetDefaultNovaProcessWithTwoSequentialUserTasks(project, user,
                updateProcess: false);

            // Find the first user task
            var firstUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the first system task following the first user task
            var firstSystemTask = novaProcess.Process.GetNextShape(firstUserTask);

            // Find the second user task following the first system task
            var secondUserTask = novaProcess.Process.GetNextShape(firstSystemTask);

            // Find the outgoing link for the first user task
            var outgoingLinkForFirstUserTask = novaProcess.Process.GetOutgoingLinkForShape(firstUserTask);

            // Add a User Decision with a branch before the first user task
            novaProcess.Process.AddUserDecisionPointWithBranchBeforeShape(firstUserTask,
                outgoingLinkForFirstUserTask.Orderindex + 1, secondUserTask.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            if (updateProcess)
            {
                novaProcess.Update(user, novaProcess.NovaProcess);
                novaProcess.RefreshArtifactFromServer(user);
            }
            return novaProcess;
        }

        /// <summary>
        /// Create and Get the Default Nova Process With Two Sequential User Tasks and One User Decision that contains Multiple Condition Branches
        /// merge before the second user task
        /// </summary>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="additionalBranches">The number of additional branches after first two branches for the user decision (main branch and first additional branch created with the user decision)</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created Nova process</returns>
        public static ProcessArtifactWrapper CreateAndGetDefaultNovaProcessWithTwoSequentialUserTasksAndOneUserDecisionContainingMultipleConditions(
            IProject project, IUser user, int additionalBranches, bool updateProcess = true)
        {
            /*
            [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--+--[UT3]--+--[ST4]--+--[E]
                           |                        |
                           +-------[UT5]--+--[ST6]--+
                           |                        |
                           +-------[UT7]--+--[ST8]--+   <--- additionalBranches: 1
            */
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));
            // Create a process with two sequential user tasks and one user decision
            var novaProcess = CreateAndGetDefaultNovaProcessWithTwoSequentialUserTasksAndOneUserDecision(project, user,
                updateProcess: false);

            // Find the precondition
            var preconditon = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the outgoing link for the precondition
            var outgoingLinkForPrecondition = novaProcess.Process.GetOutgoingLinkForShape(preconditon);

            // Find the User Decision
            var userDecision = novaProcess.Process.GetNextShape(preconditon);

            // Find the User Task From the main branch
            var userTaskFromMainBranch =
                novaProcess.Process.GetProcessShapeById(
                    novaProcess.Process.Links.Find(
                        l =>
                            l.SourceId.Equals(userDecision.Id) &&
                            l.Orderindex.Equals(outgoingLinkForPrecondition.Orderindex)).DestinationId);

            // Find the first system task following the first user task from the main branch
            var firstSystemTask = novaProcess.Process.GetNextShape(userTaskFromMainBranch);

            // Find the second user task following the first system task
            var secondUserTask = novaProcess.Process.GetNextShape(firstSystemTask);

            // branchOrderIndex for existing system decision
            var defaultAdditionalBranchOrderIndex = outgoingLinkForPrecondition.Orderindex + 2;

            for (int i = 0; i < additionalBranches; i++)
            {
                // Add branch to the existing User Decision
                novaProcess.Process.AddBranchWithUserAndSystemTaskToUserDecisionPoint(userDecision, defaultAdditionalBranchOrderIndex + i, secondUserTask.Id);
            }

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            if (updateProcess)
            {
                novaProcess.Update(user, novaProcess.NovaProcess);
                novaProcess.RefreshArtifactFromServer(user);
            }
            return novaProcess;
        }

        /// <summary>
        /// Create and Get the Default Nova Process With Two Sequential User Decisions Added
        /// </summary>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created Nova process</returns>
        public static ProcessArtifactWrapper CreateAndGetDefaultNovaProcessWithTwoSequentialUserDecisions(IProject project, IUser user, bool updateProcess = true)
        {
            /*
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--<UD2>--+--[UT3]--+--[ST4]--+--[E]
                               |                        |    |                        |
                               +-------[UT5]--+--[ST6]--+    +-------[UT7]--+--[ST8]--+
            */
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process with one user decision
            var novaProcess = CreateAndGetDefaultNovaProcessWithOneUserDecision(project, user, updateProcess: false);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Determine the branch endpoint
            var branchEndPoint = novaProcess.Process.GetNextShape(preconditionTask);

            // Add user and system task before existing user decision
            novaProcess.Process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Add Decision point with branch after precondition
            novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(
                preconditionTask,
                preconditionOutgoingLink.Orderindex + 1,
                branchEndPoint.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            if (updateProcess)
            {
                novaProcess.Update(user, novaProcess.NovaProcess);
                novaProcess.RefreshArtifactFromServer(user);
            }
            return novaProcess;
        }

        /// <summary>
        /// Create and Get the Default Nova Process With Two Sequential System Decisions Added
        /// </summary>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created Nova process</returns>
        public static ProcessArtifactWrapper CreateAndGetDefaultNovaProcessWithTwoSequentialSystemDecisions(IProject project, IUser user, bool updateProcess = true)
        {
            /*
                [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--[UT2]--<SD2>--+--[ST3]--+--[E]
                                      |              |           |              |  
                                      +----+--[ST2]--+           +----+--[ST4]--+
            */
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process with one system decision
            var novaProcess = CreateAndGetDefaultNovaProcessWithOneSystemDecision(project, user, updateProcess: false);

            // Find the End shape
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Add Additional user task along with associated system task
            var addedUserTask = novaProcess.Process.AddUserAndSystemTask(novaProcess.Process.GetIncomingLinkForShape(endShape));

            // Find the first UserTask
            var firstUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the outgoing link for the first user task
            var outgoingLinkForFirstUserTask = novaProcess.Process.GetOutgoingLinkForShape(firstUserTask);

            // Find the link between first system decision to the second branch
            var secondBranchLinkFromFirstSystemDecision = novaProcess.Process.Links.Find(
                l => l.Orderindex.Equals(outgoingLinkForFirstUserTask.Orderindex + 1)
                );

            // Find system task on the second branch of the first system decision
            var addedSystemTaskFromSecondBranch =
                novaProcess.Process.GetProcessShapeById(secondBranchLinkFromFirstSystemDecision.DestinationId);

            // Update first merging point so that first loop ends before the added user task
            novaProcess.Process.GetOutgoingLinkForShape(addedSystemTaskFromSecondBranch).DestinationId = addedUserTask.Id;

            // Find the second system task on the main branch added with additonal user task 
            var secondSystemTaskFromMainBranch = novaProcess.Process.GetNextShape(addedUserTask);

            // Add the second System Decision with branch merging to addedUserTask
            novaProcess.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(secondSystemTaskFromMainBranch, outgoingLinkForFirstUserTask.Orderindex + 1, endShape.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            if (updateProcess)
            {
                novaProcess.Update(user, novaProcess.NovaProcess);
                novaProcess.RefreshArtifactFromServer(user);
            }
            return novaProcess;
        }

        /// <summary>
        /// Create and get the default Nova Process with both a User Decision and System Decision added.
        /// </summary>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created Nova process</returns>
        public static ProcessArtifactWrapper CreateAndGetDefaultNovaProcessWithUserAndSystemDecisions(IProject project, IUser user, bool updateProcess = true)
        {
            /*
                [S]--[P]--+--<UD1>--+--[UT]---+--[ST]---+--[UT4]--<SD1>--+--[ST5]--+--[E]
                               |                        |           |              |
                               +-------[UT2]--+--[ST3]--+           +----+--[ST7]--+
            */
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process with one user decision.
            var novaProcess = CreateAndGetDefaultNovaProcessWithOneUserDecision(project, user, updateProcess: false);

            // Find the End shape.
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Add additional user & system task before the End Shape (UT4 & ST5).
            var addedUserTask = novaProcess.Process.AddUserAndSystemTask(novaProcess.Process.GetIncomingLinkForShape(endShape));

            // Find the incoming link for the last user task.
            var incomingLinkForLastUserTask = novaProcess.Process.GetIncomingLinkForShape(addedUserTask);

            // Find the link between user decision to the second branch.
            var secondBranchLinkFromUserDecision = novaProcess.Process.Links.Find(
                l => l.Orderindex.Equals(incomingLinkForLastUserTask.Orderindex + 1)
                );

            // Find user & system tasks on the second branch of the user decision.
            var secondLevelUserTask = novaProcess.Process.GetProcessShapeById(secondBranchLinkFromUserDecision.DestinationId);
            var secondLevelSystemTask = novaProcess.Process.GetNextShape(secondLevelUserTask);

            // Update first merging point so that first loop ends before the added user task.
            novaProcess.Process.GetOutgoingLinkForShape(secondLevelSystemTask).DestinationId = addedUserTask.Id;

            // Find the second system task on the main branch added with additonal user task.
            var addedSystemTask = novaProcess.Process.GetNextShape(addedUserTask);
            var incomingLinkForAddedSystemTask = novaProcess.Process.GetIncomingLinkForShape(addedSystemTask);

            // Add a System Decision with branch merging to addedUserTask (SD1 & ST7).
            novaProcess.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(addedSystemTask, incomingLinkForAddedSystemTask.Orderindex + 1, endShape.Id);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            if (updateProcess)
            {
                novaProcess.Update(user, novaProcess.NovaProcess);
                novaProcess.RefreshArtifactFromServer(user);
            }
            return novaProcess;
        }

        /// <summary>
        /// Create and Get the Default Nova Process With Two Sequential User Tasks Added
        /// </summary>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <param name="updateProcess">(optional) Update the process if true; Default = true</param>
        /// <returns>The created Nova process</returns>
        public static ProcessArtifactWrapper CreateAndGetDefaultNovaProcessWithTwoSequentialUserTasks(IProject project, IUser user, bool updateProcess = true)
        {
            /*
                [S]--[P]--+--[UT1]--+--[ST2]--+--[UT3]--+--[ST4]--+--[E]
            */

            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process
            var novaProcess = CreateAndGetDefaultNovaProcess(project, user);

            // Get the end point
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find outgoing process link for precondition
            var endPointIncomingLink = novaProcess.Process.GetIncomingLinkForShape(endShape);

            novaProcess.Process.AddUserAndSystemTask(endPointIncomingLink);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            if (updateProcess)
            {
                novaProcess.Update(user, novaProcess.NovaProcess);
                novaProcess.RefreshArtifactFromServer(user);
            }
            return novaProcess;
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
            IProject project,
            IUser user, 
            int pairs, 
            bool updateProcess = true)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create and get the default process
            var novaProcess = CreateAndGetDefaultNovaProcess(project, user);

            // Get the end point
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find incoming process link for the end shape to get the shape
            var endPointIncomingLink = novaProcess.Process.GetIncomingLinkForShape(endShape);
            var shapeBeforeEnd = novaProcess.Process.GetProcessShapeById(endPointIncomingLink.SourceId);

            // Adds x number of user tasks/system tasks pairs
            novaProcess.Process.AddXUserTaskAndSystemTask(shapeBeforeEnd, pairs);

            // If updateProcess is true, returns the updated process after the save process. If updatedProcess is false, returns the current process.
            if (updateProcess)
            {
                novaProcess.Update(user, novaProcess.Artifact);
                novaProcess.RefreshArtifactFromServer(user);
            }
            return novaProcess.Process;
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
