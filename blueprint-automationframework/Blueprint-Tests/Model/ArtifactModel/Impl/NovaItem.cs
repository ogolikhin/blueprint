using Model.ArtifactModel.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Model.ArtifactModel.Impl
{
    // Serialized JSON properties are taken from:  blueprint-current/blob/develop/Source/BluePrintSys.RC.Business.Internal/Components/Nova/Models/NovaItem.cs
    public class NovaItem
    {
        #region Serialized JSON properties

        /// <summary>
        /// This class describes which properties should be compared in the item.
        /// </summary>
        public class PropertyCompareOptions
        {
            /// <summary>Should the Id properties be compared?</summary>
            public bool CompareArtifactIds { get; set; } = true;
            /// <summary>Should the OrderIndex properties be compared?</summary>
            public bool CompareOrderIndeces { get; set; } = true;
            /// <summary>Should the Description properties be compared?</summary>
            public bool CompareDescriptions { get; set; } = true;
            /// <summary>Should the Traces properties be compared?</summary>
            public bool CompareTraces { get; set; } = true;
            /// <summary>Should the SpecificPropertyValues properties be compared?</summary>
            public bool CompareSpecificPropertyValues { get; set; } = true;
            /// <summary>Should the CustomProperties properties be compared?</summary>
            public bool CompareCustomProperties { get; set; } = true;
        }

        public int? Id { get; set; }

        //-----System Properties-----

        // This property must always be in Json, even if it is 'null'.
        // This allows our client to display an empty string and keeps it from getting default value from property type.
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string Name { get; set; }

        // This property must always be in Json, even if it is 'null'.
        // This allows our client to display an empty string and keeps it from getting default value from property type.
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string Description { get; set; }

        //---------------------------

        public int? ParentId { get; set; }
        public double? OrderIndex { get; set; }
        public int? ItemTypeId { get; set; }
        public string ItemTypeName { get; set; }
        public int? ItemTypeVersionId { get; set; }
        public int? ItemTypeIconId { get; set; }
        public string Prefix { get; set; }

        // Setter needed for deserialization.
        public List<CustomProperty> CustomPropertyValues { get; set; }

        // Setter needed for deserialization.
        public List<CustomProperty> SpecificPropertyValues { get; set; }

        public List<NovaTrace> Traces { get; set; }

        public ItemTypePredefined? PredefinedType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ItemIndicatorFlags IndicatorFlags { get; set; } = 0;

        public List<AttachmentValue> AttachmentValues { get; set; }

        public List<NovaDocumentReference> DocRefValues { get; set; }

        #endregion Serialized JSON properties
        
        // These are needed to serialize to null instead of empty lists.
        public virtual bool ShouldSerializeCustomPropertyValues()
        {
            return CustomPropertyValues.Count > 0;
        }

        public virtual bool ShouldSerializeSpecificPropertyValues()
        {
            return SpecificPropertyValues.Count > 0;
        }
        
        public bool ShouldSerializeTraces()
        {
            return Traces.Count > 0;
        }

        public bool ShouldSerializeAttachmentValues()
        {
            return AttachmentValues.Count > 0;
        }

        public bool ShouldSerializeDocRefValues()
        {
            return DocRefValues.Count > 0;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NovaItem()
        {
            CustomPropertyValues = new List<CustomProperty>();
            SpecificPropertyValues = new List<CustomProperty>();
            Traces = new List<NovaTrace>();
            AttachmentValues = new List<AttachmentValue>();
            DocRefValues = new List<NovaDocumentReference>();
        }
    }
}
