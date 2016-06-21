using System.Collections.Generic;
using System.Linq;
using System.Net;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    public class PropertyBase : IPropertyBase
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

        public OpenApiPropertyType GetPropertyType(
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
            var returnedPropertyType = artifactTypeForBaseArtifactType.PropertyTypes.Find(pt => pt.Name.Equals(propertyName));

            return returnedPropertyType;
        }
    }
}
