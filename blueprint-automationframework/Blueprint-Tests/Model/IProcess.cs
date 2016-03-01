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
        Dictionary<string, IPropertyValueInformation> PropertyValues { get; }
        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds a User Task to the Process
        /// </summary>
        /// <param name="sourceId">Id of the preceding shape</param>
        /// <param name="destinationId">Id of the following shape</param>
        /// <param name="orderIndex">Order index of the added user task (y-index)</param>
        /// <returns>The user task that was added</returns>
        IProcessShape AddUserTask(int sourceId, int destinationId, int orderIndex);

        /// <summary>
        /// Adds a User Decision Point to the Process
        /// </summary>
        /// <param name="sourceId">Id of the preceding shape</param>
        /// <param name="destinationId">Id of the following shape</param>
        /// <param name="orderIndex">Order index of the added user task (y-index)</param>
        /// <returns>The user decision point that was added</returns>
        IProcessShape AddUserDecisionPoint(int sourceId, int destinationId, int orderIndex);

        /// <summary>
        /// Adds a Branch to the Process
        /// </summary>
        /// <param name="sourceId">Id of the preceding shape</param>
        /// <param name="destinationId">Id of the following shape</param>
        /// <param name="orderIndex">Order index of the added user task (y-index)</param>
        /// <returns>The branch that was added</returns>
        IProcessShape AddBranch(int sourceId, int destinationId, int orderIndex);

        #endregion Methods
    }
}
