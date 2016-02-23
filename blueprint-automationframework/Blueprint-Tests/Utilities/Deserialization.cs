using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// ConcreteListConverter converts deserialize object based on the class which contains interface properties and converts between Lists and Arrays.
        /// </summary>
        /// <typeparam name="TI">The interface type that we are storing in the List.</typeparam>
        /// <typeparam name="TC">The concrete class that implements the TI interface.</typeparam>
        /// <example>
        ///   [JsonConverter(typeof (Deserialization.ConcreteListConverter&lt;IOpenApiProperty, OpenApiProperty&gt;))]
        ///   public List&lt;IOpenApiProperty&gt; Properties { get; set; }
        /// </example>
        public class ConcreteListConverter<TI, TC> : JsonConverter where TC : TI
        {
            public override bool CanConvert(Type objectType) => true;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                ThrowIf.ArgumentNull(serializer, nameof(serializer));

                TC[] array = serializer.Deserialize<TC[]>(reader);
                List<TC> list = array.ToList<TC>();
                return list.ConvertAll(o => (TI)o);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                ThrowIf.ArgumentNull(serializer, nameof(serializer));

                serializer.Serialize(writer, value);
            }
        }
    }
}
