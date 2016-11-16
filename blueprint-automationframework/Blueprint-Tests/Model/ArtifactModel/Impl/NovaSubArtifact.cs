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

        public int? ItemTypeVersionId { get; set; }

        public string Prefix { get; set; }

        public List<SubArtifactCustomProperty> CustomPropertyValues { get; } = new List<SubArtifactCustomProperty>();

        public List<SubArtifactCustomProperty> SpecificPropertyValues { get; } = new List<SubArtifactCustomProperty>();

        public int PredefinedType { get; set; }

        public bool ShouldSerializeAttachmentValues()
        {
            return AttachmentValues.Count > 0;
        }
        public List<AttachmentValue> AttachmentValues { get; } = new List<AttachmentValue>();

        public string DisplayName { get; set; }

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

        public class SubArtifactCustomProperty
        {
            public string Name { get; set; }

            public int PropertyTypeId { get; set; }

            public int? PropertyTypeVersionId { get; set; }

            public int PropertyTypePredefined { get; set; }

            public bool? IsMultipleAllowed { get; set; }

            public bool? IsRichText { get; set; }

            public int? PrimitiveType { get; set; }

            public object Value { get; set; }

            public bool? IsReuseReadOnly { get; set; }
        }
    }
}
