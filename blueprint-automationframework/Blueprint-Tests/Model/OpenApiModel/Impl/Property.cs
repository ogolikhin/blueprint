using System.Collections.Generic;
using System.Linq;

using System.Net;
using Utilities;

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

        private PropertyType GetPropertyType(
            IProject project,
            IUser user,
            BaseArtifactType baseArtifactType,
            string propertyName,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false
            )
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Generate ArtifactType lists for the project if the target project doesn't contains any artifact types 
            // or only contain artifact type  without property information
            if (!project.ArtifactTypes.Any() || !project.ArtifactTypes.First().PropertyTypes.Any())
            {
                project.GetAllArtifactTypes(user: user, address: Address,
                    shouldRetrievePropertyTypes: true, expectedStatusCodes: expectedStatusCodes, sendAuthorizationAsCookie: sendAuthorizationAsCookie);
            }

            // Retrive the artifactType for the selected base artifact type
            var artifactTypeForBaseArtifactType = project.ArtifactTypes.Find(at => at.BaseArtifactType.Equals(baseArtifactType));

            // Retrieve the property for the selected base artifact type based on the property name
            var returnedProperty = artifactTypeForBaseArtifactType.PropertyTypes.Find(pt => pt.Name.Equals(propertyName));

            return returnedProperty;
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
