using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Model.Factories;
using Utilities;
using System.Net;
using Utilities.Facades;
using Model.Impl;

namespace Model.OpenApiModel.Impl
{
    /// <summary>
    /// An Enumeration of possible UsersAndGroups Types
    /// </summary>
    public enum UsersAndGroupsType
    {
        User,
        Group
    }

    public class OpenApiProperty : IOpenApiProperty
    {
        #region Constants

        private const string SVC_PATH = "api/v1/projects";
        private const string URL_ARTIFACTTYPES = "metadata/artifactTypes";
        public const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";

        #endregion Constants

        #region Properties

        public int PropertyTypeId { get; set; }
        public string Name { get; set; }
        public string BasePropertyType { get; set; }
        public string TextOrChoiceValue { get; set; }
        public bool IsRichText { get; set; }
        public bool IsReadOnly { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<UsersAndGroups> UsersAndGroups { get; set; }
        public List<object> Choices { get; }
        public string DateValue { get; set; }
        public string Address { get; set; }

        #endregion Properties

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The address of the OpenApiPropertyProperty.</param>
        public OpenApiProperty(string address)
        {
            Address = address;
        }

        /// TODO: Is there any way to improve the performance for get artifactTypes call
        public OpenApiProperty SetPropertyAttribute(
            IProject project,
            IUser user,
            BaseArtifactType baseArtifactType,
            string propertyName,
            string propertyValue = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false
            )
        {

            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.OpenApiToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            RestApiFacade restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);

            var path = I18NHelper.FormatInvariant("{0}/{1}/{2}?PropertyTypes=true", SVC_PATH, project.Id, URL_ARTIFACTTYPES);

            var artifactTypes = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes, cookies: cookies);

            // Retrieve the deserialized artifact type list for the project 
            var deserializedArtifactTypes = Deserialization.DeserializeObject<List<ArtifactType>>(artifactTypes.Content);

            // Retrive the deserialized artifactType for the selected base artifact type
            var deserializedArtifactTypeForBaseArtifactType = deserializedArtifactTypes.Find(at => at.BaseArtifactType.Equals(baseArtifactType));

            // Retrieve the deserialized property for the selected base artifact type
            var deserializedReturnedProperty = deserializedArtifactTypeForBaseArtifactType.PropertyTypes.Find(pt => pt.Name.Equals(propertyName));

            // Created and update the property based on information from get artifact types call and user parameter
            var updatedProperty = new OpenApiProperty(Address)
            {
                PropertyTypeId = deserializedReturnedProperty.Id,
                Name = deserializedReturnedProperty.Name,
                BasePropertyType = deserializedReturnedProperty.BasePropertyType,
                // Set the value for the property with propertyValue parameter
                TextOrChoiceValue = propertyValue ?? deserializedReturnedProperty.DefaultValue,
                IsRichText = deserializedReturnedProperty.IsRichText,
                IsReadOnly = deserializedReturnedProperty.IsReadOnly
            };

            return updatedProperty;
        }
    }

    public class UsersAndGroups
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public UsersAndGroupsType Type { get; set; }

        public int Id { get; set; }

        public string DisplayName { get; set; }
    }
}
