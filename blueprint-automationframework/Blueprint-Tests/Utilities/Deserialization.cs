using Newtonsoft.Json;
using System;

namespace Utilities
{
    public static class Deserialization
    {

        /// <summary>
        /// ConcreteConverter converts deserialize object based on the class which contains interface properties 
        /// </summary>
        public class ConcreteConverter<T> : JsonConverter
        {
            public override bool CanConvert(Type objectType) => true;
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (serializer == null)
                {
                    throw new ArgumentNullException("serializer");
                }
                return serializer.Deserialize<T>(reader);
            }
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (serializer == null)
                {
                    throw new ArgumentNullException("serializer");
                }
                serializer.Serialize(writer, value);
            }
        }

    }
}
