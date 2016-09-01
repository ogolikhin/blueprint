using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace Model.ArtifactModel.Impl
{
    public class OpenApiProperty : PropertyBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The URL address of the Open API.</param>
        public OpenApiProperty(string address)
        {
            Address = address;
        }

        /// TODO: Is there any way to improve the performance for get artifactTypes call
        /// <summary>
        /// Set a property value for the property of the specific artifact</summary>
        /// <param name="project">the target project</param>
        /// <param name="user">blueprint user</param>
        /// <param name="baseArtifactType">base artifact Type</param>
        /// <param name="propertyName">the name of the property want to edit</param>
        /// <param name="propertyValue">(Optional) property value for the property. If null, the default value will be used if available</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
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
            return SetPropertyAttribute(Address, project, user, baseArtifactType, propertyName, propertyValue,
                expectedStatusCodes: expectedStatusCodes, sendAuthorizationAsCookie: sendAuthorizationAsCookie);
        }

        /// <summary>
        /// Set a property value for the property of the specific artifact</summary>
        /// <param name="address">The base address of the Blueprint server.</param>
        /// <param name="project">the target project</param>
        /// <param name="user">blueprint user</param>
        /// <param name="baseArtifactType">base artifact Type</param>
        /// <param name="propertyName">the name of the property want to edit</param>
        /// <param name="propertyValue">(Optional) property value for the property. If null, the default value will be used if available</param>
        /// <param name="dateValue">(optional) If this is a DateValue property, use this field to set the date.</param>
        /// <param name="usersAndGroups">(optional) If this is a UsersAndGroups property, use this field to set the UsersAndGroups.</param>
        /// <param name="choices">(optional) If this is a Choice property, use this field to set the choices.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        public static OpenApiProperty SetPropertyAttribute(
            string address,
            IProject project,
            IUser user,
            BaseArtifactType baseArtifactType,
            string propertyName,
            string propertyValue = null,
            DateTime? dateValue = null,
            List<UsersAndGroups> usersAndGroups = null,
            List<object> choices = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false
            )
        {
            // Retrieve the deserialized property for the selected base artifact type
            var returnedPropertyType = GetPropertyType(address, project, user, baseArtifactType, propertyName,
                expectedStatusCodes, sendAuthorizationAsCookie);

            // Created and update the property based on information from get artifact types call and user parameter
            var updatedProperty = new OpenApiProperty(address)
            {
                PropertyTypeId = returnedPropertyType.Id,
                Name = returnedPropertyType.Name,
                BasePropertyType = returnedPropertyType.BasePropertyType,
                // Set the value for the property with propertyValue parameter
                TextOrChoiceValue = propertyValue ?? returnedPropertyType.DefaultValue,
                DateValue = dateValue?.ToString(DateTimeFormatInfo.InvariantInfo),
                UsersAndGroups = usersAndGroups,
                IsRichText = returnedPropertyType.IsRichText ?? false,
                IsReadOnly = returnedPropertyType.IsReadOnly
            };

            if (choices != null)
            {
                updatedProperty.Choices?.Clear();
                updatedProperty.Choices?.AddRange(choices);
            }

            return updatedProperty;
        }
    }
}
