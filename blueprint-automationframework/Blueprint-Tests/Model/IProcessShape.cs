using System.Collections.Generic;
using Model.Impl;

namespace Model
{
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
        /// Sub artifact Id for the shape
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Name of the shape
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Parent Id of the shape
        /// </summary>
        int ParentId { get; set; }

        /// <summary>
        /// Project containing the Process
        /// </summary>
        int ProjectId { get; set; }

        /// <summary>
        /// Prefix of the process type
        /// </summary>
        string TypePrefix { get; set; }

        /// <summary>
        /// Artifact associated with the process shape (i.e. the Include) 
        /// </summary>
        ArtifactPathLink AssociatedArtifact { get; set; }

        /// <summary>
        /// Base item type for the process artifact
        /// </summary>
        ItemTypePredefined BaseItemTypePredefined { get; set; }

        /// <summary>
        /// The property values for the Process shape
        /// </summary>
        Dictionary<string, PropertyValueInformation> PropertyValues { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="artifact"></param>
        /// <returns></returns>
        IArtifactPathLink AddAssociatedArtifact(IOpenApiArtifact artifact);

        #endregion Properties
    }
}
