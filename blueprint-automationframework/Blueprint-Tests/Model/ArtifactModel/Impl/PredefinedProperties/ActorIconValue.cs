using System.Collections.Generic;
using Model.Common.Enums;
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

        /// <summary>
        /// Returns IconAddress. Works for JSON returned from server.
        /// expected address /svc/bpartifactstore/diagram/actoricon/{artifactId}?versionId={versionId}&addDraft=true&lastSavedTimestamp={TimeStamp}
        /// </summary>
        /// <returns>string IconAddress - /svc/bpartifactstore/diagram/actoricon/{artifactId}?versionId={versionId}&addDraft=true&lastSavedTimestamp={TimeStamp}</returns>
        public string GetIconAddress()
        {
            if (IconValue.ContainsKey(outIconKey))
            {
                return IconValue[outIconKey].ToString();
            }

            return null;
        }

        /// <summary>
        /// Sets IconValue. Works for JSON to be sent to server.
        /// </summary>
        /// <param name="fileGuid">Guid of the file uploaded to FileStore.</param>
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

        public static readonly PropertyTypePredefined PropertyType = PropertyTypePredefined.Image;

        private const string outIconKey = "url";
        private const string inIconKey = "guid";
    }
}
