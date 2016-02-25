using System.Collections.Generic;

namespace Model
{
    public interface IStorytellerUserStory
    {
        int Id { get; set; }
        string Name { get; set; }
        int ProjectId { get; set; }
        int TypeId { get; set; }
        string typePrefix { get; set; }
        PropertyTypePredefined TypePredefined { get; set; }
        List<IOpenApiProperty> SystemProperties { get; }
        List<IOpenApiProperty> CustomProperties { get; }

        int? ProcessTaskId { get; set; }
        bool? IsNew { get; set; }
    }
}
