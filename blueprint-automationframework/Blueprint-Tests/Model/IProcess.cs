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

        /// <summary>
        /// Parameterless constructor required for deserialization
        /// </summary>
        void Process();
    }
}
