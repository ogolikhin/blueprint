using Newtonsoft.Json;
using System.Collections.Generic;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    public class NovaSubArtifact : INovaSubArtifact
    {
        #region Serialized JSON Properties

        public bool? IsDeleted { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int? ParentId { get; set; }

        public double? OrderIndex { get; set; }

        public int? ItemTypeId { get; set; }

        public string DisplayName { get; set; }

        public int? ItemTypeVersionId { get; set; }

        public int PredefinedType { get; set; }

        public string Prefix { get; set; }

        public bool ShouldSerializeCustomPropertyValues()
        {
            return CustomPropertyValues.Count > 0;
        }
        public List<CustomProperty> CustomPropertyValues { get; } = new List<CustomProperty>();

        public bool ShouldSerializeSpecificPropertyValues()
        {
            return SpecificPropertyValues.Count > 0;
        }
        public List<CustomProperty> SpecificPropertyValues { get; } = new List<CustomProperty>();

        public bool ShouldSerializeAttachmentValues()
        {
            return AttachmentValues.Count > 0;
        }
        public List<AttachmentValue> AttachmentValues { get; } = new List<AttachmentValue>();
        
        public bool HasChildren { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteListConverter<INovaSubArtifact, NovaSubArtifact>))]
        public List<INovaSubArtifact> Children { get; set; } = new List<INovaSubArtifact>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<NovaTrace> Traces { get; set; }

        public bool ShouldSerializeChildren()
        {
            return Children.Count > 0;
        }

        #endregion Serialized JSON Properties
    }
}
