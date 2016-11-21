using Model.ArtifactModel.Enums;

namespace Model.ArtifactModel.Impl
{
    // Found in: blueprint-current/Source/BluePrintSys.RC.Service.Business/Repository/Models/Storyteller/ArtifactReference.cs
    public class ArtifactReference
    {
        /// <summary>
        /// The Id of the artifact
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The project Id for the artifact
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// The name of the artifact
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type prefix for the artifact
        /// </summary>
        public string TypePrefix { get; set; }

        /// <summary>
        /// The name of the project
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// The base item type for the artifact
        /// </summary>
        public ItemTypePredefined BaseItemTypePredefined { get; set; }

        /// <summary>
        /// The version of the artifact
        /// </summary>
        public int? Version { get; set; }        
        
        /// <summary>
        /// The link to navigate to the artifact
        /// </summary>
        public string Link { get; set; }


    }
}
