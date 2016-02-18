﻿namespace Model
{
    public interface IArtifactReference
    {
        #region Properties

        /// <summary>
        /// The Id of the Artifact
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// The Project Id for the artifact
        /// </summary>
        int ProjectId { get; set; }

        /// <summary>
        /// The name of the artifact
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The type prefix for the artifact
        /// </summary>
        string TypePreffix { get; set; }

        /// <summary>
        /// The type prefix for the artifact
        /// </summary>
        ItemTypePredefined BaseItemTypePredefined { get; set; }

        /// <summary>
        /// The link to navigate to the artifact
        /// </summary>
        string Link { get; set; }

        #endregion Properties
    }
}
