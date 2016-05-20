using Model.ArtifactModel.Impl;
using System.Collections.Generic;
using System.Net;

namespace Model.ArtifactModel
{
    public interface IOpenApiProperty :IPropertyBase
    {
        int PropertyTypeId { get; set; }
        string Name { get; set; }
        string BasePropertyType { get; set; }
        string TextOrChoiceValue { get; set; }
        bool IsRichText { get; set; }
        bool IsReadOnly { get; set; }
        List<UsersAndGroups> UsersAndGroups { get; }
        List<object> Choices { get; }
        string DateValue { get; set; }

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
        OpenApiProperty SetPropertyAttribute(IProject project, IUser user, BaseArtifactType baseArtifactType,
            string propertyName, string propertyValue = null, List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false);

    }
}
