using System.Collections.Generic;
using Model.Impl;

namespace Model
{
    public enum ProcessType
    {
        None = 0,
        BusinessProcess = 1,
        UserToSystemProcess = 2,
        SystemToSystemProcess = 3
    }

    public interface IProcess
    {
        #region Properties

        /// <summary>
        /// Project containing the Process
        /// </summary>
        int ProjectId { get; set; }

        /// <summary>
        /// Artifact Id for the process
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Name for the process
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Prefix of the process type
        /// </summary>
        string TypePrefix { get; set; }

        /// <summary>
        /// Base item type for the process artifact
        /// </summary>
        ItemTypePredefined BaseItemTypePredefined { get; set; }

        /// <summary>
        /// Sub-artifact shapes for the process
        /// </summary>
        List<ProcessShape> Shapes { get; }

        /// <summary>
        /// Sub-artifact links for the process
        /// </summary>
        List<ProcessLink> Links { get; }

        /// <summary>
        /// Artifact path links for the Process.  This supports breadcrumb navigation
        /// </summary>
        List<ArtifactPathLink> ArtifactPathLinks { get; }

        /// <summary>
        /// The property values for the Process artifact
        /// </summary>
        Dictionary<string, PropertyValueInformation> PropertyValues { get; }
        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds a User Task to the Process
        /// </summary>
        /// <param name="processLink">The process link where the user task will be added</param>
        /// <returns>The user task that was added</returns>
        IProcessShape AddUserTask(IProcessLink processLink);

        /// <summary>
        /// Adds a User Decision Point to the Process
        /// </summary>
        /// <param name="processLink">The process link where the user decision point will be added</param>
        /// <returns>The user decision point that was added</returns>
        IProcessShape AddUserDecisionPoint(IProcessLink processLink);

        /// <summary>
        /// Adds a Branch to a Decision Point
        /// </summary>
        /// <param name="decisionPointId">Id of the decision point</param>
        /// <param name="orderIndex">Order index of the added branch</param>
        /// <param name="destinationId">Id of the following shape</param>
        /// <returns>The user task created</returns>
        IProcessShape AddBranchWithUserTaskToDecisionPoint(int decisionPointId, double orderIndex, int destinationId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="destinationId"></param>
        /// <param name="orderIndex"></param>
        /// <returns></returns>
        IProcessLink AddLink(int sourceId, int destinationId, double orderIndex);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idOfPreviousShape"></param>
        /// <param name="orderIndexOfBranch"></param>
        /// <param name="idOfBranchMergePoint"></param>
        /// <returns></returns>
        IProcessShape AddDecisionPointWithBranchAfterShape(int idOfPreviousShape, double orderIndexOfBranch, int? idOfBranchMergePoint = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idOfNextShape"></param>
        /// <param name="orderIndexOfBranch"></param>
        /// <param name="idOfBranchMergePoint"></param>
        /// <returns></returns>
        IProcessShape AddDecisionPointWithBranchBeforeShape(int idOfNextShape, double orderIndexOfBranch, int? idOfBranchMergePoint = null);

        /// <summary>
        /// Find the Process Shape by the Shape Name
        /// </summary>
        /// <param name="shapeName">The shape name</param>
        /// <returns>The process shape</returns>
        IProcessShape FindProcessShapeByShapeName(string shapeName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shapeId"></param>
        /// <returns></returns>
        IProcessShape FindProcessShapeById(int shapeId);

        /// <summary>
        /// Find the Incoming Process Link for a Shape
        /// </summary>
        /// <param name="shapeId">The shape id</param>
        /// <returns>The process link</returns>
        IProcessLink FindIncomingLinkForShape(int shapeId);

        /// <summary>
        /// Find the Outgoing Process Link for a Shape
        /// </summary>
        /// <param name="shapeId">The shape id</param>
        /// <returns>The process link</returns>
        IProcessLink FindOutgoingLinkForShape(int shapeId);

        #endregion Methods
    }
}
