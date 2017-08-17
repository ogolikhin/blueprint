using System.Collections.Generic;
using System.Xml.Serialization;

// Originates from Raptor solution with some modifications
namespace ServiceLibrary.Repositories.ProjectMeta.PropertyXml.Models
{
    [XmlType]
    public class XmlCustomProperty
    {
        [XmlAttribute(AttributeName = "Id")]
        public string PropertyTypeId { get; set; }

        [XmlAttribute(AttributeName = "T")]
        public string PrimitiveType { get; set; }

        [XmlAttribute(AttributeName = "N")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "R")]
        public string Required { get; set; }

        [XmlAttribute(AttributeName = "AC")]
        public string AllowCustomValue { get; set; }

        [XmlAttribute(AttributeName = "AM")]
        public string AllowMultipleValues { get; set; }

        [XmlAttribute(AttributeName = "SId")]
        public string StandardPropertyTypeId { get; set; }

        private List<XmlCustomPropertyValidValue> _validValues;
        [XmlArray(ElementName = "VVS")]
        [XmlArrayItem(ElementName = "VV")]
        public List<XmlCustomPropertyValidValue> ValidValues => _validValues ?? (_validValues = new List<XmlCustomPropertyValidValue>());
        public static XmlCustomProperty CreateAsValue(int propertyTypeId, int primitiveType)
        {
            return new XmlCustomProperty()
            {
                PropertyTypeId = XmlModelConvert.FromInt32(propertyTypeId),
                PrimitiveType = XmlModelConvert.FromInt32(primitiveType)
            };
        }
    }
}
