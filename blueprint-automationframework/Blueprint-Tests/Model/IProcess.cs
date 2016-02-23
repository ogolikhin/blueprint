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
        /// Parent Id for the process
        /// </summary>
        int ParentId { get; set; }

        /// <summary>
        /// Connections and states for the process
        /// </summary>
        uint ConnectionsAndStates { get; set; }

        /// <summary>
        /// The order index of the process artifact
        /// </summary>
        double OrderIndex { get; set; }

        /// <summary>
        /// Type Id for the process artifact
        /// </summary>
        int TypeId { get; set; }

        /// <summary>
        /// Prefix of the process type
        /// </summary>
        string TypePreffix { get; set; }

        /// <summary>
        /// Base item type for the process artifact
        /// </summary>
        ItemTypePredefined BaseItemTypePredefined { get; set; }

        /// <summary>
        /// Version Id of the process
        /// </summary>
        int VersionId { get; set; }

        /// <summary>
        /// User id of the the user that has a lock on the Process
        /// </summary>
        int? LockedByUserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int? ArtifactInfoParentId { get; set; }

        /// <summary>
        /// Permissions for the artifact
        /// </summary>
        int Permissions { get; set; }

        /// <summary>
        /// Artifact display Id
        /// </summary>
        int ArtifactDisplayId { get; set; }

        /// <summary>
        /// Byte array for the process thumbnail
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        byte[] Thumbnail { get; set; }

        /// <summary>
        /// Sub-artifact shapes for the process
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        List<ProcessShape> Shapes { get; }

        /// <summary>
        /// Sub-artifact links for the process
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        List<ProcessLink> Links { get; }

        /// <summary>
        /// Artifact path links for the Process.  This supports breadcrumb navigation
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        List<ArtifactPathLink> ArtifactPathLinks { get; }

        /// <summary>
        /// The property values for the Process artifact
        /// </summary>
        Dictionary<string, PropertyValueInformation> PropertyValues { get; }
        #endregion Properties
    }
}
