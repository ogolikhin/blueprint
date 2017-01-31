using System.Collections.Generic;
using System.Net;

namespace Model.ArtifactModel.Impl
{
    public class Property : PropertyBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The URL address of the API.</param>
        public Property(string address)
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
        /// <returns>A Property with the specified values.</returns>
        public Property SetPropertyAttribute(
            IProject project,
            IUser user,
            BaseArtifactType baseArtifactType,
            string propertyName,
            string propertyValue = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            // Retrieve the deserialized property for the selected base artifact type
            var returnedPropertyType = GetPropertyType(project, user, baseArtifactType, propertyName, expectedStatusCodes);

            // Created and update the property based on information from get artifact types call and user parameter
            var updatedProperty = new Property(Address)
            {
                PropertyTypeId = returnedPropertyType.Id,
                Name = returnedPropertyType.Name,
                BasePropertyType = returnedPropertyType.BasePropertyType,
                // Set the value for the property with propertyValue parameter
                TextOrChoiceValue = propertyValue ?? returnedPropertyType.DefaultValue,
                IsRichText = returnedPropertyType.IsRichText ?? false,
                IsReadOnly = returnedPropertyType.IsReadOnly
            };

            return updatedProperty;
        }


    }
}
