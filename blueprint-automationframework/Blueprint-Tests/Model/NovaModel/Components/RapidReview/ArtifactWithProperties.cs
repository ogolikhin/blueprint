using System.Collections.Generic;

// Found in:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/RapidReview/Models/ArtifactWithProperties.cs
namespace Model.NovaModel.Components.RapidReview
{
    public class ArtifactWithProperties
    {
        public int ArtifactId { get; set; }
        public IList<ArtifactProperty> Properties { get; set; }
        public IList<ArtifactProperty> AuthorHistory { get; set; }
        public ArtifactProperty Description { get; set; }
    }

    public class ArtifactProperty
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public PropertyValueFormat Format { get; set; }
        public int PropertyTypeId { get; set; }
    }

    public enum PropertyValueFormat
    {
        Text,
        Html,
        Date,
        DateTimeUtc
    }
}
