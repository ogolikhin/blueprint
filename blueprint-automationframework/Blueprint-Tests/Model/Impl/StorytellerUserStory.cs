using Newtonsoft.Json;
using System.Collections.Generic;
using Utilities;

namespace Model.Impl
{
    public class StorytellerUserStory : IStorytellerUserStory
    {
        #region Properties

        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public int TypeId { get; set; }
        public string typePrefix { get; set; }
        public PropertyTypePredefined TypePredefined { get; set; }
        [JsonConverter(typeof(Deserialization.ConcreteListConverter<IOpenApiProperty, OpenApiProperty>))]
        public List<IOpenApiProperty> SystemProperties { get; } = new List<IOpenApiProperty>();
        [JsonConverter(typeof(Deserialization.ConcreteListConverter<IOpenApiProperty, OpenApiProperty>))]
        public List<IOpenApiProperty> CustomProperties { get; } = new List<IOpenApiProperty>();

        public int? ProcessTaskId { get; set; }
        public bool? IsNew { get; set; }

        #endregion Properties

        public StorytellerUserStory()
        {
            //Required for deserializing OpenApiUserStoryArtifact
        }
    }
}
