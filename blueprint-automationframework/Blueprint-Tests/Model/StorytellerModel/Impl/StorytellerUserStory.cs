using System.Net;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utilities;
using Utilities.Facades;
using Common;
using Model.Common.Enums;
using Model.Impl;
using NUnit.Framework;

namespace Model.StorytellerModel.Impl
{
    public class StorytellerUserStory : IStorytellerUserStory
    {
        private const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";

        #region Properties

        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public int TypeId { get; set; }
        public string TypePrefix { get; set; }
        public PropertyTypePredefined TypePredefined { get; set; }

        [JsonConverter(typeof(SerializationUtilities.ConcreteConverter<List<StorytellerProperty>>))]
        public List<StorytellerProperty> SystemProperties { get; set; }

        [JsonConverter(typeof(SerializationUtilities.ConcreteConverter<List<StorytellerProperty>>))]
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

        /// <summary>
        /// Asserts that the properties of the two IStorytellerUserStory objects are equal.
        /// </summary>
        /// <param name="expectedUserStory">The expected IStorytellerUserStory.</param>
        /// <param name="actualUserStory">The actual IStorytellerUserStory.</param>
        /// <param name="skipIds">(optional) Pass true to skip comparison of the Id properties.</param>
        /// <exception cref="AssertionException">If any properties don't match.</exception>
        public static void AssertAreEqual(IStorytellerUserStory expectedUserStory,
            IStorytellerUserStory actualUserStory,
            bool skipIds = false)
        {
            ThrowIf.ArgumentNull(expectedUserStory, nameof(expectedUserStory));
            ThrowIf.ArgumentNull(actualUserStory, nameof(actualUserStory));

            if (!skipIds)
            {
                Assert.AreEqual(expectedUserStory.Id, actualUserStory.Id, "The Id properties don't match!");
                Assert.AreEqual(expectedUserStory.ProcessTaskId, actualUserStory.ProcessTaskId, "The ProcessTaskId properties don't match!");
            }

            Assert.AreEqual(expectedUserStory.Name, actualUserStory.Name, "The Name properties don't match!");
            Assert.AreEqual(expectedUserStory.ProjectId, actualUserStory.ProjectId, "The ProjectId properties don't match!");
            Assert.AreEqual(expectedUserStory.TypeId, actualUserStory.TypeId, "The TypeId properties don't match!");
            Assert.AreEqual(expectedUserStory.TypePrefix, actualUserStory.TypePrefix, "The TypePrefix properties don't match!");
            Assert.AreEqual(expectedUserStory.TypePredefined, actualUserStory.TypePredefined, "The TypePredefined properties don't match!");
            Assert.AreEqual(expectedUserStory.IsNew, actualUserStory.IsNew, "The IsNew properties don't match!");

            Assert.AreEqual(expectedUserStory.CustomProperties.Count, actualUserStory.CustomProperties.Count,
                "The number of CustomProperties is different!");

            foreach (var expectedCustomProperty in expectedUserStory.CustomProperties)
            {
                var actualCustomProperty = actualUserStory.CustomProperties.Find(p => p.Name.Equals(expectedCustomProperty.Name));

                Assert.NotNull(actualCustomProperty, "Couldn't find an actual Custom Property named: {0}", expectedCustomProperty.Name);
                StorytellerProperty.AssertAreEqual(expectedCustomProperty, actualCustomProperty, skipIds);
            }

            Assert.AreEqual(expectedUserStory.SystemProperties.Count, actualUserStory.SystemProperties.Count,
                "The number of SystemProperties is different!");

            foreach (var expectedSystemProperty in expectedUserStory.SystemProperties)
            {
                var actualCustomProperty = actualUserStory.SystemProperties.Find(p => p.Name.Equals(expectedSystemProperty.Name));

                Assert.NotNull(actualCustomProperty, "Couldn't find an actual System Property named: {0}", expectedSystemProperty.Name);
                StorytellerProperty.AssertAreEqual(expectedSystemProperty, actualCustomProperty, skipIds);
            }
        }

        public UpdateResult<StorytellerProperty> UpdateNonfunctionalRequirements(string address,
            IUser user,
            string value,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            var path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.RapidReview.Items_id_.PROPERTIES, Id);
                        
            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            var nonFunctionalRequirementProperty = CustomProperties.First(property => property.Name.StartsWithOrdinal("ST-Non-Functional Requirements"));   // Use StartsWith() instead of == because it might have "(Agile Pack)" on the end of the name.
            nonFunctionalRequirementProperty.Value = value;
            
            var restApi = new RestApiFacade(address, tokenValue);

            var userstoryUpdateResult = restApi.SendRequestAndDeserializeObject<UpdateResult<StorytellerProperty>, List<StorytellerProperty>>(
                path,
                RestRequestMethod.PATCH, 
                jsonObject: new List<StorytellerProperty>(){ nonFunctionalRequirementProperty }, 
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return userstoryUpdateResult;
        }
    }
}
