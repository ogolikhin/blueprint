using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RestSharp.Serializers;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using Common;
using Newtonsoft.Json.Linq;

namespace Utilities
{
    public static class SerializationUtilities
    {
        /// <summary>
        /// Either casts or deserializes the specified object into a new type, depending on whether the object is a JSON object or now.
        /// </summary>
        /// <typeparam name="T">The type to convert the object into.</typeparam>
        /// <param name="valueToConvert">The object to be converted.</param>
        /// <returns>The converted object.</returns>
        public static T CastOrDeserialize<T>(object valueToConvert)
        {
            if (valueToConvert is Newtonsoft.Json.Linq.JObject)
            {
                return JsonConvert.DeserializeObject<T>(valueToConvert.ToString());
            }

            return (T)valueToConvert;
        }

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

                // Distionary keys must be serialized with a capitla first letter in order to match the
                // Process model in the backend
                Dictionary<string, TI> dict2 =  new Dictionary<string, TI>();

                foreach (var kvp in (Dictionary<string, TI>)value)
                {
                    string newKey = char.ToUpper(kvp.Key[0], CultureInfo.InvariantCulture) + kvp.Key.Substring(1);
                    dict2.Add(newKey, kvp.Value); 
                }

                serializer.Serialize(writer, dict2);
            }
        }

        /// <summary>
        /// A custom serializer to override the default RestSharp serializer
        /// </summary>
        public class CustomJsonSerializer : ISerializer
        {
            public CustomJsonSerializer()
            {
                ContentType = "application/json";
            }

            public string Serialize(object obj)
            {
                return JsonConvert.SerializeObject(obj, Formatting.None,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore});
            }

            public string RootElement { get; set; }
            public string Namespace { get; set; }
            public string DateFormat { get; set; }
            public string ContentType { get; set; }
        }

        /// <summary>
        /// Deserialize JSON Content to Generic Type
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <param name="content">The JSON body content</param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content);
        }

        /// <summary>
        /// Check that server's JSON matches type T from the test Framework.
        /// </summary>
        /// <typeparam name="T">The expected type of the deserialized JSON object</typeparam>
        /// <param name="deserializedObjectFromServer">Result of deserialization using Framework presentation of T.</param>
        /// <param name="serializedObjectFromServer">String presentation of object T received from Blueprint server.</param>
        /// <exception cref="FormatException">A FormatException if JSON has been changed.</exception>
        public static void CheckJson<T>(T deserializedObjectFromServer, string serializedObjectFromServer)
        {
            if (deserializedObjectFromServer == null && serializedObjectFromServer == null) return;

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            string serializedObject = JsonConvert.SerializeObject(deserializedObjectFromServer, jsonSerializerSettings);

            bool isJsonEqual = CompareJsonStrings(serializedObject, serializedObjectFromServer);

            if (!isJsonEqual)
            {
                string msg = I18NHelper.FormatInvariant("JSON for {0} has been changed!\r\nReceived JSON:   {1}\r\nSerialized JSON: {2}",
                    typeof(T).ToString(), serializedObjectFromServer, serializedObject);
                throw new FormatException(msg);
            }
        }

        /// <summary>
        /// Compare 2 JSON strings for equality (order of tokens not important)
        /// </summary>
        /// <param name="json1">The first JSON string to compare.</param>
        /// <param name="json2">The second JSON string to compare.</param>
        /// <returns>True is strings are equal; False otherwise</returns>
        public static bool CompareJsonStrings(string json1, string json2)
        {
            ThrowIf.ArgumentNull(json1, nameof(json1));
            ThrowIf.ArgumentNull(json2, nameof(json2));

            var jsonToken1 = JToken.Parse(json1.ToLower(CultureInfo.CurrentCulture));
            var jsonToken2 = JToken.Parse(json2.ToLower(CultureInfo.CurrentCulture));

            return JToken.DeepEquals(jsonToken1, jsonToken2);
        }

        /// <summary>
        /// Chech whether string is a serialized JSON object
        /// </summary>
        /// <param name="jsonString">String to check.</param>
        /// <returns>True if string is a serialized JSON object, false otherwise.</returns>
        public static bool IsStringAJson(string jsonString)
        {
            try
            {
                JToken.Parse(jsonString);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }
    }
}
