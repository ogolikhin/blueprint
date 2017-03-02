namespace Model.StorytellerModel
{
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