using Model.Impl;
using System.Collections.Generic;

namespace Model
{
    public interface IStorytellerUserStory
    {
        int Id { get; set; }
        string Name { get; set; }
        int ProjectId { get; set; }
        int TypeId { get; set; }
        string TypePrefix { get; set; }
        PropertyTypePredefined TypePredefined { get; set; }
        List<StorytellerProperty> SystemProperties { get; }
        List<StorytellerProperty> CustomProperties { get; }

        int ProcessTaskId { get; set; }
        bool IsNew { get; set; }
    }

    public interface IStorytellerProperty
    {
        string Name { get; set; }
        int PropertyTypeId { get; set; }
        //int? PropertyType { get; set; }
        string Value { get; set; }
    }
}
