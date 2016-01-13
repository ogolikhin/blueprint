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
                ThrowIf.ArgumentNull(serializer, nameof(serializer));

                return serializer.Deserialize<T>(reader);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                ThrowIf.ArgumentNull(serializer, nameof(serializer));

                serializer.Serialize(writer, value);
            }
        }
    }
}
