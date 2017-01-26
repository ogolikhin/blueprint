using System.Collections.Generic;
using Newtonsoft.Json;

namespace Model.ArtifactModel.Impl.PredefinedProperties
{
    public class ActorIconValue
    {
        // TODO: find a way to prohibit use of ActorIcon for everything except JSON serializer
        // http://www.newtonsoft.com/json/help/html/SerializationAttributes.htm 
        // serialize "value":{"guid":"6784efa6-ffe3-e611-810a-12ee66db398f"} instead of "value":{"ActorIcon":{"guid":"6ecb9156-dee3-e611-810a-12ee66db398f"}}
        [JsonExtensionData]
        public Dictionary<string, object> IconValue { get; set; }

        public string GetIconAddress()
        {
            if (IconValue.ContainsKey(outIconKey))
            {
                return IconValue[outIconKey].ToString();
            }
            else
                return null;
        }

        public void SetIcon (string fileGuid)
        {
            if (IconValue == null)
            {
                IconValue = new Dictionary<string, object>();
            }

            if (IconValue.ContainsKey(inIconKey))
            {
                IconValue[inIconKey] = fileGuid;
            }
            else
            {
                IconValue.Add(inIconKey, fileGuid);
            }
            // TODO: add code to delete other keys from dictionary?
            // TODO: add code which adds imageSource key with base64 encoded image (?!) to mock fron-end behaviour
        }

        private const string outIconKey = "url";
        private const string inIconKey = "guid";
    }
}
