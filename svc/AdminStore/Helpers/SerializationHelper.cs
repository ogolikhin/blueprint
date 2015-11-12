using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace AdminStore.Helpers
{
    internal static class SerializationHelper
    {
        [DataContract]
        internal class FASettings
        {
            public FASettings()
            {
                NameClaimType = "Username";
            }

            [DataMember]
            public string LoginUrl { get; set; }

            [DataMember]
            public string LogoutUrl { get; set; }

            [DataMember]
            public string ErrorUrl { get; set; }

            [DataMember]
            public string LoginPrompt { get; set; }

            [DataMember]
            public string ESigPrompt { get; set; }

            [DataMember]
            public string NameClaimType  { get; set; }
        }

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

        internal static string Serialize<T>(T data)
        {
            if (data == null)
            {
                return string.Empty;
            }

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