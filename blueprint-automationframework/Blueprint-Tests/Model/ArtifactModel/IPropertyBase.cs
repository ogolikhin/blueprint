using System.Collections.Generic;
using System.Net;
using Model.ArtifactModel.Impl;

namespace Model.ArtifactModel
{
    public interface IPropertyBase
    {
        PropertyType GetPropertyType(
            IProject project,
            IUser user,
            BaseArtifactType baseArtifactType,
            string propertyName,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false
            );
    }
}