using System.Collections.Generic;
using Model.StorytellerModel.Impl;

namespace Model.StorytellerModel
{

    public interface IStorytellerUserStory
    {
        /// <summary>
        /// Id of the UserStory
        /// </summary>
        int Id { get; set; }
        /// <summary>
        /// Name of the UserStory
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// ProejctId of the UserStory
        /// </summary>
        int ProjectId { get; set; }
        /// <summary>
        /// TypeId of the UserStory
        /// </summary>
        int TypeId { get; set; }
        /// <summary>
        /// TypePrefix of the UserStory
        /// </summary>
        string TypePrefix { get; set; }
        /// <summary>
        /// PropertyTypePredefined of the UserStory
        /// </summary>
        PropertyTypePredefined TypePredefined { get; set; }
        /// <summary>
        /// SystemProperties of the UserStory
        /// </summary>
        List<StorytellerProperty> SystemProperties { get; }
        /// <summary>
        /// CustomProperties of the UserStory
        /// </summary>
        List<StorytellerProperty> CustomProperties { get; }
        /// <summary>
        /// ProcessTaskId of the UserStory
        /// </summary>
        int ProcessTaskId { get; set; }
        /// <summary>
        /// IsNew of the UserStory - Inidicator if the UserStory is created or updated
        /// </summary>
        bool IsNew { get; set; }
    }

    public interface IStorytellerProperty
    {
        /// <summary>
        /// Id of the StorytellerPropert
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Id of the StorytellerPropert
        /// </summary>
        int PropertyTypeId { get; set; }
        /// <summary>
        /// PropertyType of the StorytellerPropert
        /// </summary>
        int? PropertyType { get; set; }
        /// <summary>
        /// Value of the StorytellerPropert
        /// </summary>
        string Value { get; set; }
    }
}
