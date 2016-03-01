using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RestSharp.Serializers;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

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

        /// <summary>
        /// ConcreteDictionaryConverter converts a dictionary based upon the concrete dictionary class and the value type inferface
        /// </summary>
        /// <typeparam name="TC">The concrete dictionary class</typeparam>
        /// <typeparam name="TI">The value type interface</typeparam>
        public class ConcreteDictionaryConverter<TC, TI> : JsonConverter
        {
            public override bool CanConvert(Type objectType) => true;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                ThrowIf.ArgumentNull(serializer, nameof(serializer));

                return serializer.Deserialize<TC>(reader);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                ThrowIf.ArgumentNull(serializer, nameof(serializer));
                ThrowIf.ArgumentNull(writer, nameof(writer));

                Dictionary<string, TI> dict2 =  new Dictionary<string, TI>();

                foreach (var kvp in (Dictionary<string, TI>)value)
                {
                    string newKey = kvp.Key.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture) + kvp.Key.Substring(1);
                    dict2.Add(newKey, kvp.Value); 
                }

                serializer.Serialize(writer, dict2);
            }
        }

        /// <summary>
        /// A custom serializer to override the default RestSharp serializer
        /// </summary>
        public class CustomSerializer : ISerializer
        {
            public CustomSerializer()
            {
                ContentType = "application/json";
            }

            public string Serialize(object obj)
            {
                return JsonConvert.SerializeObject(obj, Formatting.None);
            }

            public string RootElement { get; set; }
            public string Namespace { get; set; }
            public string DateFormat { get; set; }
            public string ContentType { get; set; }
        }
    }
}
