using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdminStore.Helpers
{
    /// <summary>Generic JSON Converter. It requires the implementer to override a Create method when implement converter for specific types.</summary>
    public abstract class JsonTypeConverter<T> : JsonConverter
    {
        /// <summary>Create specific object based on properties in the JObject</summary>
        /// <param name="objectType">Expected type of object.</param>
        /// <param name="jObject">JSON object's content.</param>
        protected abstract T Create(Type objectType, JObject jObject);

        /// <summary>Determines if the type is supported.</summary>
        /// <param name="objectType">Type for deserialization.</param>
        /// <returns>True if the type is supported.</returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        /// <summary>Parse the json to the specific object type.</summary>
        /// <param name="reader">JsonReader</param>
        /// <param name="objectType">Base type of object.</param>
        /// <param name="existingValue">existingValue</param>
        /// <param name="serializer">JsonSerializer.</param>
        /// <returns>Deserialized Object</returns>
        public override object ReadJson(JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var jObject = JObject.Load(reader);

            // Create specific object based on JObject
            var specificTypeObject = Create(objectType, jObject);
            if (specificTypeObject == null)
                return null;

            // fill properties
            using (var jObjectReader = CopySettingsToNewReader(reader, jObject))
            {
                serializer.Populate(jObjectReader, specificTypeObject);
            }
            return specificTypeObject;
        }

        /// <summary>Serializes to the specified type</summary>
        /// <param name="writer">JsonWriter</param>
        /// <param name="value">Object to serialize.</param>
        /// <param name="serializer">JsonSerializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        /// <summary>Create a new reader for jObject and copy existing reader's settings.</summary>
        /// <param name="reader">The existing reader.</param>
        /// <param name="jObject">The jObject.</param>
        /// <returns>The new reader.</returns>
        public static JsonReader CopySettingsToNewReader(JsonReader reader, JObject jObject)
        {
            var newReader = jObject.CreateReader();
            newReader.Culture = reader.Culture;
            newReader.DateFormatString = reader.DateFormatString;
            newReader.DateParseHandling = reader.DateParseHandling;
            newReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
            newReader.FloatParseHandling = reader.FloatParseHandling;
            newReader.MaxDepth = reader.MaxDepth;
            newReader.SupportMultipleContent = reader.SupportMultipleContent;
            return newReader;
        }
    }
}