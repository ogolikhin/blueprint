using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Utilities;
using Utilities.Facades;
using Common;
using Model.Impl;

namespace Model.StorytellerModel.Impl
{
    public class StorytellerUserStory : IStorytellerUserStory
    {
        private const string SVC_PATH = "svc/components/RapidReview";
        private const string items = "items";
        private const string properties = "properties";
        private const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";
        #region Properties

        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public int TypeId { get; set; }
        public string TypePrefix { get; set; }
        public PropertyTypePredefined TypePredefined { get; set; }
        [SuppressMessage("Microsoft.Usage",
    "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //[JsonConverter(typeof(Deserialization.ConcreteListConverter<IStorytellerProperty, StorytellerProperty>))]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<StorytellerProperty>>))]
        public List<StorytellerProperty> SystemProperties { get; set; }
        [SuppressMessage("Microsoft.Usage",
    "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //[JsonConverter(typeof(Deserialization.ConcreteListConverter<IStorytellerProperty, StorytellerProperty>))]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<StorytellerProperty>>))]
        public List<StorytellerProperty> CustomProperties { get; set; }

        public int ProcessTaskId { get; set; }
        public bool IsNew { get; set; }

        #endregion Properties

        public StorytellerUserStory()
        {
            //Required for deserializing OpenApiUserStoryArtifact
            SystemProperties = new List<StorytellerProperty>();
            CustomProperties = new List<StorytellerProperty>();
        }

        public UpdateResult<StorytellerProperty> UpdateNonfunctionalRequirements(string address, IUser user, string value, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            var path = I18NHelper.FormatInvariant("{0}/{1}/{2}/{3}", SVC_PATH, items, Id, properties);
                        
            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            var nonFunctionalRequirementProperty = CustomProperties.First(property => property.Name == "ST-Non-Functional Requirements");
            nonFunctionalRequirementProperty.Value = value;
            
            RestApiFacade restApi = new RestApiFacade(address, user.Username, user.Password, tokenValue);

            var userstoryUpdateResult = restApi.SendRequestAndDeserializeObject<UpdateResult<StorytellerProperty>, List<StorytellerProperty>>(path,
                RestRequestMethod.PATCH, jsonObject: new List<StorytellerProperty>(){ nonFunctionalRequirementProperty }, expectedStatusCodes: expectedStatusCodes);
            return userstoryUpdateResult;
        }
    }

    public class StorytellerProperty : IStorytellerProperty
    {
        public string Name { get; set; }
        public int PropertyTypeId { get; set; }
        public int? PropertyType { get; set; }
        public string Value { get; set; }
    }
}
