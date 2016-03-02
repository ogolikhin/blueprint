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
        /// <param name="sourceId">Id of the preceding shape</param>
        /// <param name="destinationId">Id of the following shape</param>
        /// <returns>The user task that was added</returns>
        IProcessShape AddUserTask(int sourceId, int destinationId);

        /// <summary>
        /// Adds a User Decision Point to the Process
        /// </summary>
        /// <param name="sourceId">Id of the preceding shape</param>
        /// <param name="destinationId">Id of the following shape</param>
        /// <returns>The user decision point that was added</returns>
        IProcessShape AddUserDecisionPoint(int sourceId, int destinationId);

        /// <summary>
        /// Adds a Branch to the Process
        /// </summary>
        /// <param name="sourceId">Id of the preceding shape</param>
        /// <param name="destinationId">Id of the following shape</param>
        /// <param name="orderIndex">Order index of the added user task (y-index)</param>
        void AddBranch(int sourceId, int destinationId, double orderIndex);

        /// <summary>
        /// Find the Process Shape by the Shape Name
        /// </summary>
        /// <param name="shapeName">The shape name</param>
        /// <returns>The process shape</returns>
        IProcessShape FindProcessShapeByShapeName(string shapeName);

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
