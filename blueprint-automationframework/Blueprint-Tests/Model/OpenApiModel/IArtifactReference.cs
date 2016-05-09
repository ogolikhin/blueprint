namespace Model.OpenApiModel
{
    public interface IArtifactReference
    {
        /// <summary>
        /// The Id of the artifact
        /// </summary>
        int Id { get; set; }
        
        /// <summary>
        /// The project Id for the artifact
        /// </summary>
        int ProjectId { get; set; }
        
        /// <summary>
        /// The name of the artifact
        /// </summary>
        string Name { get; set; }
        
        /// <summary>
        /// The type prefix for the artifact
        /// </summary>
        string TypePrefix { get; set; }
        
        /// <summary>
        /// The base item type for the artifact
        /// </summary>
        ItemTypePredefined BaseItemTypePredefined { get; set; }

        /// <summary>
        /// The link to navigate to the artifact
        /// </summary>
        string Link { get; set; }  
    }
}
