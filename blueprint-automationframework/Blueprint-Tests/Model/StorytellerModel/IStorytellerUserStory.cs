using System.Collections.Generic;
using Model.StorytellerModel.Impl;

namespace Model.StorytellerModel
{

    public interface IStorytellerUserStory
    {
        /// <summary>
        /// The Artifact Id of the User Story
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Name of the User Story
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Project Id of the User Story
        /// </summary>
        int ProjectId { get; set; }

        /// <summary>
        /// TypeId of the User Story
        /// </summary>
        int TypeId { get; set; }

        /// <summary>
        /// TypePrefix of the User Story
        /// </summary>
        string TypePrefix { get; set; }

        /// <summary>
        /// The Predefined Property Type of the User Story
        /// </summary>
        PropertyTypePredefined TypePredefined { get; set; }

        /// <summary>
        /// List of System Properties of the User Story
        /// </summary>
        List<StorytellerProperty> SystemProperties { get; }

        /// <summary>
        /// List of Custom Properties of the User Story
        /// </summary>
        List<StorytellerProperty> CustomProperties { get; }

        /// <summary>
        /// ProcessTaskId of the User Story
        /// </summary>
        int ProcessTaskId { get; set; }

        /// <summary>
        /// Boolean flag indicating whether the User Story is being Created or Updated
        /// </summary>
        bool IsNew { get; set; }
    }

    public interface IStorytellerProperty
    {
        /// <summary>
        /// Name of the Storyteller Property
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Type Id of the Storyteller Property
        /// </summary>
        int PropertyTypeId { get; set; }

        /// <summary>
        /// PropertyType of the Storyteller Property
        /// </summary>
        int? PropertyType { get; set; }

        /// <summary>
        /// Value of the Storyteller Property
        /// </summary>
        string Value { get; set; }
    }
}
