using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ServiceLibrary.Helpers
{
    public static class SerializationHelper
    {
        public static T Deserialize<T>(string xml)
        {
            return Deserialize<T>(xml, Encoding.Unicode);
        }

        public static T Deserialize<T>(string xml, Encoding encoding)
        {
            using (var stream = new MemoryStream(encoding.GetBytes(xml)))
            {
                var serializer = new DataContractSerializer(typeof(T));
                var theObject = (T)serializer.ReadObject(stream);
                return theObject;
            }
        }

        public static bool TryDeserialize<T>(string xml, out T result)
        {
            try
            {
                result = Deserialize<T>(xml);
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }

        public static string Serialize<T>(T data)
        {
            var serializer = new DataContractSerializer(typeof(T));

            var result = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(result))
            {
                serializer.WriteObject(xmlWriter, data);
            }

            return result.ToString();
        }

        // Copied from Blueprint
        public static T FromXml<T>(string xml) where T : class
        {
            if (string.IsNullOrEmpty(xml))
            {
                return null;
            }

            try
            {
                using (var reader = XmlReader.Create(new StringReader(xml)))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    var obj = serializer.Deserialize(reader);
                    return obj as T;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Copied from Blueprint
        public static string ToXml(object obj, bool indent = false)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            var serializer = new XmlSerializer(obj.GetType());
            var builder = new StringBuilder();
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = indent };

            using (var writer = XmlWriter.Create(builder, settings))
            {
                serializer.Serialize(writer, obj, ns);
            }

            return builder.ToString();
        }

        public static T FromXml<T>(Stream xml) where T : class
        {
            if (xml.CanSeek)
            {
                xml.Position = 0;
            }

            var serializer = new XmlSerializer(typeof(T));
            var obj = serializer.Deserialize(xml) as T;
            return obj;
        }
    }
}
