using System.Collections.Generic;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.StorytellerModel.Impl;

namespace Model.StorytellerModel
{
    /// <summary>
    /// Enumeration of Process Shape Types
    /// </summary>
    public enum ProcessShapeType
    {
        None = 0,
        Start = 1,
        UserTask = 2,
        End = 3,
        SystemTask = 4,
        PreconditionSystemTask = 5,
        UserDecision = 6,
        SystemDecision = 7
    }

    public interface IProcessShape
    {
        #region Properties

        /// <summary>
        /// Sub artifact Id for the process shape
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Name of the process shape
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Parent Id of the process shape
        /// </summary>
        int ParentId { get; set; }

        /// <summary>
        /// Project containing the process shape
        /// </summary>
        int ProjectId { get; set; }

        /// <summary>
        /// Type prefix of the process shape
        /// </summary>
        string TypePrefix { get; set; }

        /// <summary>
        /// Artifact reference associated with the process shape (i.e. the Include) 
        /// </summary>
        AssociatedArtifact AssociatedArtifact { get; set; }

        /// <summary>
        /// Base item type for the process shape
        /// </summary>
        ItemTypePredefined BaseItemTypePredefined { get; set; }

        /// <summary>
        /// The property values for the process shape
        /// </summary>
        Dictionary<string, PropertyValueInformation> PropertyValues { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Add Associated Artifact Reference (Include) to Process Shape
        /// </summary>
        /// <param name="artifact">The artifact to add</param>
        /// <returns>The artifact reference to the associated artifact</returns>
        AssociatedArtifact AddAssociatedArtifact(IArtifact artifact);

        /// <summary>
        /// Verify if the Process is specific processShapeType
        /// </summary>
        /// <param name="processShapeType">The processShapeType</param>
        /// <returns>Returns true if the process's processShapeType equals to the processShapeType passed as parameter</returns>
        bool IsTypeOf(ProcessShapeType processShapeType);

        #endregion Methods
    }
}
