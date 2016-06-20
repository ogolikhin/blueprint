using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace ServiceLibrary.Helpers
{
    public class SerializationHelper
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
    }
}
