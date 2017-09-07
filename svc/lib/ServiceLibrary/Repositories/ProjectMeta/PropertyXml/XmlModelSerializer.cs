using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ServiceLibrary.Repositories.ProjectMeta.PropertyXml.Models;

namespace ServiceLibrary.Repositories.ProjectMeta.PropertyXml
{
    // Originates from Raptor solution with some modifications
    public static class XmlModelSerializer
    {
        #region Common

        private static readonly XmlSerializerNamespaces _serializerNamespaces;
        private static readonly XmlWriterSettings _writerSettings;

        static XmlModelSerializer()
        {
            _serializerNamespaces = new XmlSerializerNamespaces();
            _serializerNamespaces.Add(string.Empty, string.Empty);
            _writerSettings = new XmlWriterSettings()
            {
                OmitXmlDeclaration = true,
                Indent = false
            };
        }

        #endregion Common

        #region CustomProperties

        private static readonly XmlSerializer _customPropertiesSerializer = new XmlSerializer(typeof(XmlCustomProperties));

        public static string SerializeCustomProperties(XmlCustomProperties customProperties)
        {
            if (customProperties == null)
            {
                return null;
            }
            StringBuilder stringBuilder = new StringBuilder(0x100);
            using (XmlWriter writer = XmlWriter.Create(stringBuilder, _writerSettings))
            {
                _customPropertiesSerializer.Serialize(writer, customProperties, _serializerNamespaces);
            }
            return stringBuilder.ToString();
        }

        public static XmlCustomProperties DeserializeCustomProperties(string customPropertyChar)
        {
            if (customPropertyChar == null)
            {
                return null;
            }
            using (StringReader stringReader = new StringReader(customPropertyChar))
            {
                return (XmlCustomProperties)_customPropertiesSerializer.Deserialize(XmlReader.Create(stringReader));
            }
        }

        #endregion CustomProperties
    }
}
