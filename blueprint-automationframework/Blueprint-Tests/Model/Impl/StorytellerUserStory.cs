using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        public string TypePrefix { get; set; }
        public PropertyTypePredefined TypePredefined { get; set; }
        [SuppressMessage("Microsoft.Usage",
    "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //[JsonConverter(typeof(Deserialization.ConcreteListConverter<IStorytellerProperty, StorytellerProperty>))]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<StorytellerProperty>>))]
        public List<StorytellerProperty> SystemProperties { get; set; }
        [SuppressMessage("Microsoft.Usage",
    "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //[JsonConverter(typeof(Deserialization.ConcreteListConverter<IStorytellerProperty, StorytellerProperty>))]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<StorytellerProperty>>))]
        public List<StorytellerProperty> CustomProperties { get; set; }

        public int ProcessTaskId { get; set; }
        public bool IsNew { get; set; }

        #endregion Properties

        public StorytellerUserStory()
        {
            //Required for deserializing OpenApiUserStoryArtifact
            SystemProperties = new List<StorytellerProperty>();
            CustomProperties = new List<StorytellerProperty>();
        }
    }

    public class StorytellerProperty : IStorytellerProperty
    {
        public string Name { get; set; }
        public int PropertyTypeId { get; set; }
        //public int? PropertyType { get; set; }
        public string Value { get; set; }
    }
}
