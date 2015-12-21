using System;
using Newtonsoft.Json;

namespace Model.Impl
{
    public class ArtifactResult : IArtifactResult
    {
        [JsonConverter(typeof(ConcreteConverter<Artifact>))]
        public IArtifact Artifact { get; set; }
        public string Message { get; set; }
        public string ResultCode { get; set; }

        private class ConcreteConverter<T> : JsonConverter
        {
            public override bool CanConvert(Type objectType) => true;
            public override object ReadJson(JsonReader reader,
             Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (serializer == null)
                {
                    throw new ArgumentNullException("serializer");
                }
                return serializer.Deserialize<T>(reader);
            }
            public override void WriteJson(JsonWriter writer,
                object value, JsonSerializer serializer)
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
