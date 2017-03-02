using System.Collections.Generic;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.StorytellerModel.Enums;
using Model.StorytellerModel.Impl;

namespace Model.StorytellerModel
{
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
        /// Base item type for the process shape
        /// </summary>
        ItemTypePredefined BaseItemTypePredefined { get; set; }

        /// <summary>
        /// The property values for the process shape
        /// </summary>
        Dictionary<string, PropertyValueInformation> PropertyValues { get; }

        /// <summary>
        /// Artifact reference associated with the process shape (i.e. the Include) 
        /// </summary>
        ArtifactReference AssociatedArtifact { get; set; }

        /// <summary>
        /// Persona reference associated with the process shape 
        /// </summary>
        ArtifactReference PersonaReference { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Add Associated Artifact Reference (Include) to Process Shape
        /// </summary>
        /// <param name="artifact">The Nova artifact to add</param>
        /// <returns>The artifact reference to the associated artifact</returns>
        ArtifactReference AddAssociatedArtifact(INovaArtifactDetails artifact);

        /// <summary>
        /// Add Persona Reference to Process Shape
        /// </summary>
        /// <param name="artifact">The Nova artifact to add</param>
        /// <returns>The artifact reference to the persona</returns>
        ArtifactReference AddPersonaReference(INovaArtifactDetails artifact);

        /// <summary>
        /// Adds a default Persona reference (User or System).
        /// </summary>
        /// <param name="processShapeType">The type of the process shape that will have the default persona reference.</param>
        /// <returns>The default artifact reference.</returns>
        ArtifactReference AddDefaultPersonaReference(ProcessShapeType processShapeType);

        /// <summary>
        /// Gets the shape type of this ProcessShape.
        /// </summary>
        /// <returns>The type of shape this shape is.</returns>
        ProcessShapeType GetShapeType();

        /// <summary>
        /// Verify if the Process is specific processShapeType
        /// </summary>
        /// <param name="processShapeType">The processShapeType</param>
        /// <returns>Returns true if the process's processShapeType equals to the processShapeType passed as parameter</returns>
        bool IsTypeOf(ProcessShapeType processShapeType);

        #endregion Methods
    }
}
