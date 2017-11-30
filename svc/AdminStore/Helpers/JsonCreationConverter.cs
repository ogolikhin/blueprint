using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdminStore.Helpers
{
    /// <summary>Base Generic JSON Converter that can help quickly define converters for specific types by automatically
    /// generating the CanConvert, ReadJson, and WriteJson methods, requiring the implementer only to define a strongly typed Create method.</summary>
    public abstract class JsonCreationConverter<T> : JsonConverter
    {
        /// <summary>Create an instance of objectType, based properties in the JSON object</summary>
        /// <param name="objectType">type of object expected</param>
        /// <param name="jObject">contents of JSON object that will be deserialized</param>
        protected abstract T Create(Type objectType, JObject jObject);

        /// <summary>Determines if this converted is designed to deserialization to objects of the specified type.</summary>
        /// <param name="objectType">The target type for deserialization.</param>
        /// <returns>True if the type is supported.</returns>
        public override bool CanConvert(Type objectType)
        {
            // FrameWork 4.5
            // return typeof(T).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
            // Otherwise
            return typeof(T).IsAssignableFrom(objectType);
        }

        /// <summary>Parses the json to the specified type.</summary>
        /// <param name="reader">Newtonsoft.Json.JsonReader</param>
        /// <param name="objectType">Target type.</param>
        /// <param name="existingValue">Ignored</param>
        /// <param name="serializer">Newtonsoft.Json.JsonSerializer to use.</param>
        /// <returns>Deserialized Object</returns>
        public override object ReadJson(JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);
            // Create target object based on JObject
            T target = Create(objectType, jObject);
            if (target == null) return null;
            // Populate the object properties
            using (JsonReader jObjectReader = CopyReaderForObject(reader, jObject))
            {
                serializer.Populate(jObjectReader, target);
            }
            return target;
        }

        /// <summary>Serializes to the specified type</summary>
        /// <param name="writer">Newtonsoft.Json.JsonWriter</param>
        /// <param name="value">Object to serialize.</param>
        /// <param name="serializer">Newtonsoft.Json.JsonSerializer to use.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        /// <summary>Creates a new reader for the specified jObject by copying the settings
        /// from an existing reader.</summary>
        /// <param name="reader">The reader whose settings should be copied.</param>
        /// <param name="jObject">The jObject to create a new reader for.</param>
        /// <returns>The new disposable reader.</returns>
        public static JsonReader CopyReaderForObject(JsonReader reader, JObject jObject)
        {
            JsonReader jObjectReader = jObject.CreateReader();
            jObjectReader.Culture = reader.Culture;
            jObjectReader.DateFormatString = reader.DateFormatString;
            jObjectReader.DateParseHandling = reader.DateParseHandling;
            jObjectReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
            jObjectReader.FloatParseHandling = reader.FloatParseHandling;
            jObjectReader.MaxDepth = reader.MaxDepth;
            jObjectReader.SupportMultipleContent = reader.SupportMultipleContent;
            return jObjectReader;
        }
    }
}