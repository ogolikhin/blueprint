using System.Collections.Generic;
using System.Net;
using Model.ArtifactVersionModel.Impl;
using Model.OpenApiModel;

namespace Model.ArtifactVersionModel
{
    public interface IArtifactVersion
    {
        IOpenApiArtifact CreateAndSaveProcessArtifact(
            IProject project,
            BaseArtifactType artifactType,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null);

        List<LockResultInfo> LockArtifactIds(IUser user, List<int> artifactIds, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        List<DiscardResultInfo> Discard(List<int> artifactIds);

    }
}