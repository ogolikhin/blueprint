using System.Collections.Generic;
using System.Linq;
using System.Net;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    public class PropertyBase
    {
        #region Properties

        public int PropertyTypeId { get; set; }                 // OpenAPI-Add-Get
        public string Name { get; set; }                        // OpenAPI-Add-Get
        public string BasePropertyType { get; set; }            // OpenAPI-Add-Get
        public string TextOrChoiceValue { get; set; }           // OpenAPI-Add-Get
        public bool IsRichText { get; set; }                    // OpenAPI-Add-Get
        public bool IsReadOnly { get; set; }                    // OpenAPI-Get
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<UsersAndGroups> UsersAndGroups { get; set; }// OpenAPI-Add-Get
        public List<object> Choices { get; }
        public string DateValue { get; set; }                   // OpenAPI-Get
        public string Address { get; set; }

        #endregion Properties

        /// <summary>
        /// Get the Property Type of a Property
        /// </summary>
        /// <param name="project">The project that contains the property type</param>
        /// <param name="user">The user making the request</param>
        /// <param name="baseArtifactType">The base artifact type of the property being requested</param>
        /// <param name="propertyName">The name of the property for which the property type is being requested</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The property type of the property</returns>
        public OpenApiPropertyType GetPropertyType(
            IProject project,
            IUser user,
            BaseArtifactType baseArtifactType,
            string propertyName,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false
            )
        {
            return GetPropertyType(Address, project, user, baseArtifactType, propertyName, expectedStatusCodes, sendAuthorizationAsCookie);
        }

        /// <summary>
        /// Get the Property Type of a Property
        /// </summary>
        /// <param name="address">The base address of the Blueprint server.</param>
        /// <param name="project">The project that contains the property type</param>
        /// <param name="user">The user making the request</param>
        /// <param name="baseArtifactType">The base artifact type of the property being requested</param>
        /// <param name="propertyName">The name of the property for which the property type is being requested</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The property type of the property</returns>
        public static OpenApiPropertyType GetPropertyType(
            string address,
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
                project.GetAllArtifactTypes(user: user, address: address,
                    shouldRetrievePropertyTypes: true, expectedStatusCodes: expectedStatusCodes, sendAuthorizationAsCookie: sendAuthorizationAsCookie);
            }

            // Retrive the artifactType for the selected base artifact type
            var artifactTypeForBaseArtifactType = project.ArtifactTypes.Find(at => at.BaseArtifactType.Equals(baseArtifactType));

            // Retrieve the property for the selected base artifact type based on the property name
            var returnedPropertyType = artifactTypeForBaseArtifactType.PropertyTypes.Find(pt => pt.Name.Equals(propertyName));

            return returnedPropertyType;
        }
    }
}
