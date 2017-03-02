using System.Collections.Generic;
using Model.ArtifactModel.Enums;
using Model.StorytellerModel.Enums;
using Model.StorytellerModel.Impl;

namespace Model.StorytellerModel
{
    public interface IProcess
    {
        #region Properties

        /// <summary>
        /// Project containing the Process
        /// </summary>
        int ProjectId { get; set; }

        /// <summary>
        /// Artifact Id of the Process
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Name of the Process
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Type Prefix of the Process
        /// </summary>
        string TypePrefix { get; set; }

        /// <summary>
        /// Base Item Type for the Process
        /// </summary>
        ItemTypePredefined BaseItemTypePredefined { get; set; }

        /// <summary>
        /// Sub-artifact Process Shapes for the Process
        /// </summary>
        List<ProcessShape> Shapes { get; }

        /// <summary>
        /// Sub-artifact Process Links for the Process
        /// </summary>
        List<ProcessLink> Links { get; }

        /// <summary>
        /// Decision branch destination links for decision shapes in the Process. This list contains list of merge point
        /// information for all available decisions in the process. The list is empty if the process contain only main branch
        /// </summary>
        List<DecisionBranchDestinationLink> DecisionBranchDestinationLinks { get; }


        /// <summary>
        /// Status contains the process status information that resides on the server side
        /// </summary>
        ProcessStatus Status { get; set; }

        /// <summary>
        /// The Artifact version information
        /// </summary>
        VersionInfo RequestedVersionInfo { get; set; }

        /// <summary>
        /// The Property values for the Process
        /// </summary>
        Dictionary<string, PropertyValueInformation> PropertyValues { get; }

        /// <summary>
        /// The Process Type of the Process
        /// </summary>
        ProcessType ProcessType { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Add a User Task and System Task to the Process
        /// </summary>
        /// <param name="processLink">The process link where the user task will be added</param>
        /// <returns>The user task that was added</returns>
        IProcessShape AddUserAndSystemTask(ProcessLink processLink);

        /// <summary>
        /// Add a Branch to a User Decision Point
        /// </summary>
        /// <param name="decisionPoint">The user decision point</param>
        /// <param name="orderIndex">Order index of the added branch (Indicates display order 
        /// in the process graph)</param>
        /// <param name="destinationId">The artifact Id of the following process shape</param>
        /// <returns>The user task created</returns>
        IProcessShape AddBranchWithUserAndSystemTaskToUserDecisionPoint(
            IProcessShape decisionPoint, 
            double orderIndex, 
            int destinationId);

        /// <summary>
        /// Add a Link to a Process
        /// </summary>
        /// <param name="sourceId">The artifact Id of the link source</param>
        /// <param name="destinationId">The artifact Id of the link destination</param>
        /// <param name="orderIndex">The order index of the link (Indicates display order 
        /// in the process graph)</param>
        /// <returns>The process link that was added</returns>
        ProcessLink AddLink(int sourceId, int destinationId, double orderIndex);

        /// <summary>
        /// Add a DecisionBranchDestinationLink to a Process
        /// </summary>
        /// <param name="destinationId">The artifact Id of the link destination</param>
        /// <param name="orderIndex">The order index of the link (Indicates display order 
        /// in the process graph)</param>
        /// <param name="sourceId">The Id of the source decision</param>
        /// <returns>The DecisionBranchDestinationLink that was added</returns>
        DecisionBranchDestinationLink AddDecisionBranchDestinationLink(int destinationId,
            double orderIndex, int sourceId);

        /// <summary>
        /// Change a branch merge point of the decision
        /// </summary>
        /// <param name="decisionPoint">The user or system decision point</param>
        /// <param name="orderIndex">The order index of the link (Indicates display order 
        /// in the process graph)</param>
        /// <param name="branchMergeLink">The existing merge link for the branch</param>
        /// <param name="mergePoint">The new merge shape</param>
        void ChangeBranchMergePoint(IProcessShape decisionPoint, double orderIndex,
            ProcessLink branchMergeLink, IProcessShape mergePoint);

        /// <summary>
        /// Add a User Decision Point with a Branch After an Existing Shape
        /// </summary>
        /// <param name="previousShape">The shape before the insertion point</param>
        /// <param name="orderIndexOfBranch">The order index of the added branch (Indicates 
        /// display order in the process graph)</param>
        /// <param name="idOfBranchMergePoint">(optional) The artifact Id of the shape where 
        /// the branch terminates</param>
        /// <returns>The user decision point that was added</returns>
        IProcessShape AddUserDecisionPointWithBranchAfterShape(
            IProcessShape previousShape, 
            double orderIndexOfBranch, 
            int? idOfBranchMergePoint = null);

        /// <summary>
        /// Add a User Decision Point with a Branch Before an Existing Shape
        /// </summary>
        /// <param name="nextShape">The shape after the insertion point</param>
        /// <param name="orderIndexOfBranch">The order index of the added branch (Indicates
        ///  display order in the process graph)</param>
        /// <param name="idOfBranchMergePoint">(optional) The artifact Id of the shape where
        ///  the branch terminates</param>
        /// <returns>The user decision point that was added</returns>
        IProcessShape AddUserDecisionPointWithBranchBeforeShape(
            IProcessShape nextShape, 
            double orderIndexOfBranch, 
            int? idOfBranchMergePoint = null);

        /// <summary>
        /// Add a System Decision Point with a Branch Before an Existing System Task
        /// </summary>
        /// <param name="nextSystemTaskShape">The system task after the insertion point</param>
        /// <param name="orderIndexOfBranch">The order index of the added branch (Indicates
        ///  display order in the process graph)</param>
        /// <param name="idOfBranchMergePoint">(optional) The artifact Id of the shape where
        ///  the branch terminates</param>
        /// <returns>The system decision point that was added</returns>
        IProcessShape AddSystemDecisionPointWithBranchBeforeSystemTask(
            IProcessShape nextSystemTaskShape, 
            double orderIndexOfBranch, 
            int? idOfBranchMergePoint = null);

        /// <summary>
        /// Add a Branch to a System Decision Point
        /// </summary>
        /// <param name="decisionPoint">The system decision point</param>
        /// <param name="orderIndex">Order index of the added branch (Indicates display order
        ///  in the process graph)</param>
        /// <param name="destinationId">The artifact Id of the following process shape</param>
        void AddBranchWithSystemTaskToSystemDecisionPoint(
            IProcessShape decisionPoint, 
            double orderIndex, 
            int destinationId);

        /// <summary>
        /// Adds x number of pairs of User Task and System Task after a shape.
        /// </summary>
        /// <param name="processShape">User tasks/system tasks will be added after this shape.</param>
        /// <param name="numberOfPairs">The number of pairs of user tasks/system tasks to add</param>
        IProcessShape AddXUserTaskAndSystemTask(IProcessShape processShape, int numberOfPairs);

        /// <summary>
        /// Get the Process Shape by the Shape Name
        /// </summary>
        /// <param name="shapeName">The shape name</param>
        /// <returns>The process shape object</returns>
        IProcessShape GetProcessShapeByShapeName(string shapeName);

        /// <summary>
        /// Get the Process Shape by the Artifact Id of the Shape
        /// </summary>
        /// <param name="shapeId">The artifact Id of the shape</param>
        /// <returns>The process shape object</returns>
        IProcessShape GetProcessShapeById(int shapeId);

        /// <summary>
        /// Get the Process Shape Type by the Artifact Id of the Shape
        /// </summary>
        /// <param name="shapeId">The artifact Id of the shape</param>
        /// <returns>The process shape type</returns>
        ProcessShapeType GetProcessShapeTypeById(int shapeId);

        /// <summary>
        /// Get list of process shapes by process shapeType
        /// </summary>
        /// <param name="processShapeType">The process shapeType</param>
        /// <returns>The list of process shapes</returns>
        List<IProcessShape> GetProcessShapesByShapeType(ProcessShapeType processShapeType);

        /// <summary>
        /// Get the Incoming Process Link for a Shape
        /// </summary>
        /// <param name="processShape">The process shape</param>
        /// <returns>The incoming process link</returns>
        ProcessLink GetIncomingLinkForShape(IProcessShape processShape);

        /// <summary>
        /// Get the Incoming Process Links for a Shape
        /// </summary>
        /// <param name="processShape">The process shape</param>
        /// <returns>The incoming process links</returns>
        List<ProcessLink> GetIncomingLinksForShape(IProcessShape processShape);

        /// <summary>
        /// Get the Outgoing Process Link for a Shape
        /// </summary>
        /// <param name="processShape">The process shape</param>
        /// <param name="orderIndex">(optional) The order index of the link to find</param>
        /// <returns>The outgoing process link</returns>
        ProcessLink GetOutgoingLinkForShape(IProcessShape processShape, double? orderIndex = null);

        /// <summary>
        /// Get the Outgoing Process Links for a Shape
        /// </summary>
        /// <param name="processShape">The process shape</param>
        /// <returns>The outgoing process links</returns>
        List<ProcessLink> GetOutgoingLinksForShape(IProcessShape processShape);

        /// <summary>
        /// Get the Shape Following an Existing Shape
        /// </summary>
        /// <param name="shape">The existing shape</param>
        /// <param name="orderIndex">(optional) The order index of the link to find</param>
        /// <returns>The shape following the existing shape</returns>
        IProcessShape GetNextShape(IProcessShape shape, double? orderIndex = null);

        /// <summary>
        /// Get the DecisionBranchDestinationLink for the decision
        /// </summary>
        /// <param name="decisionShape">The source decision shape for the DecisionBranchDestinationLink looking for</param>
        /// <param name="orderIndex">The order index of the branch based from the shource decision shape</param>
        /// <returns>The DecisionBranchDestinationLink the source decision</returns>
        DecisionBranchDestinationLink GetDecisionBranchDestinationLinkForDecisionShape(IProcessShape decisionShape,
            double orderIndex);

        /// <summary>
        /// Delete a System Decision with all Branches that are Not of the Lowest Order
        /// </summary>
        /// <param name="systemDecision">The system decision point to be deleted</param>
        /// <param name="mergePointShape">The shape where all the associated branches terminate</param>
        void DeleteSystemDecisionWithBranchesNotOfTheLowestOrder(
            IProcessShape systemDecision,
            IProcessShape mergePointShape);

        /// <summary>
        /// Delete a System Decision Branch
        /// </summary>
        /// <param name="systemDecision">The system decision containing the branch</param>
        /// <param name="orderIndex">The order index of the branch</param>
        /// <param name="branchMergePointShape">The end point of the branch</param>
        void DeleteSystemDecisionBranch(IProcessShape systemDecision, double orderIndex,
            IProcessShape branchMergePointShape);

        /// <summary>
        /// Delete a User and Associated System task
        /// </summary>
        /// <param name="userTask">The user task to be deleted</param>
        void DeleteUserAndSystemTask(IProcessShape userTask);

        /// <summary>
        /// Delete a User Task and Associated System Task Including All Subsequent System 
        /// Decision Branches and Shapes
        /// </summary>
        /// <param name="userTask">The user task to delete</param>
        /// <param name="mergePointShape">The shape where all the associated branches terminate</param>
        void DeleteUserAndSystemTaskWithAllBranches(IProcessShape userTask, IProcessShape mergePointShape);

        /// <summary>
        /// Delete a User Decision with all Branches that are Not of the Lowest Order
        /// </summary>
        /// <param name="userDecision">The user decision point to be deleted</param>
        /// <param name="mergePointShape">The shape where all the associated branches terminate</param>
        void DeleteUserDecisionWithBranchesNotOfTheLowestOrder(
            IProcessShape userDecision,
            IProcessShape mergePointShape);

        /// <summary>
        /// Delete a User Decision Branch
        /// </summary>
        /// <param name="userDecision">The user decision containing the branch</param>
        /// <param name="orderIndex">The order index of the branch</param>
        /// <param name="branchMergePointShape">The end point of the branch</param>
        void DeleteUserDecisionBranch(IProcessShape userDecision, double orderIndex, IProcessShape branchMergePointShape);

        /// <summary>
        /// Delete a DecisionBranchDestinationLink from a Process
        /// </summary>
        /// <param name="destinationId">The artifact Id of the link destination</param>
        /// <param name="orderIndex">The order index of the link (Indicates display order 
        /// in the process graph)</param>
        /// <param name="sourceId">The Id of the source decision</param>
        void DeleteDecisionBranchDestinationLink(int destinationId, double orderIndex, int sourceId);

        /// <summary>
        /// Delete all available DecisionBranchDestinationLinks for the source decision
        /// </summary>
        /// <param name="decision">The source decision</param>
        void DeleteDecisionBranchDestinationLinksForDecision(IProcessShape decision);

        /// <summary>
        /// Move a User and System Task to Before a Shape
        /// </summary>
        /// <param name="userTaskToMove">The user task to move</param>
        /// <param name="destinationShape">The shape that will follow the moved user and system task</param>
        void MoveUserAndSystemTaskBeforeShape(IProcessShape userTaskToMove, IProcessShape destinationShape);

        /// <summary>
        /// Move a User and System Task to After a Shape
        /// </summary>
        /// <param name="userTaskToMove">The user task to move</param>
        /// <param name="sourceShape">The shape that will precede the moved user and system task</param>
        void MoveUserAndSystemTaskAfterShape(IProcessShape userTaskToMove, IProcessShape sourceShape);

        #endregion Methods
    }
}
