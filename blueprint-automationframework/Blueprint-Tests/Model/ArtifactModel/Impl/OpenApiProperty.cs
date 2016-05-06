using System.Collections.Generic;
using System.Linq;

using System.Net;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    public class OpenApiProperty : PropertyBase, IOpenApiProperty
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The address of the OpenApiPropertyProperty.</param>
        public OpenApiProperty(string address)
        {
            Address = address;
        }

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
            // Retrieve the deserialized property for the selected base artifact type
            var returnedPropertyType = GetPropertyType(project, user, baseArtifactType, propertyName,
                expectedStatusCodes, sendAuthorizationAsCookie);

            // Created and update the property based on information from get artifact types call and user parameter
            var updatedProperty = new OpenApiProperty(Address)
            {
                PropertyTypeId = returnedPropertyType.Id,
                Name = returnedPropertyType.Name,
                BasePropertyType = returnedPropertyType.BasePropertyType,
                // Set the value for the property with propertyValue parameter
                TextOrChoiceValue = propertyValue ?? returnedPropertyType.DefaultValue,
                IsRichText = returnedPropertyType.IsRichText,
                IsReadOnly = returnedPropertyType.IsReadOnly
            };

            return updatedProperty;
        }
    }
}
