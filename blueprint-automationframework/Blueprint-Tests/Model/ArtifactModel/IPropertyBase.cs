using System.Collections.Generic;
using System.Net;
using Model.ArtifactModel.Impl;

namespace Model.ArtifactModel
{
    public interface IPropertyBase
    {
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
        OpenApiPropertyType GetPropertyType(
            IProject project,
            IUser user,
            BaseArtifactType baseArtifactType,
            string propertyName,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false
            );
    }
}