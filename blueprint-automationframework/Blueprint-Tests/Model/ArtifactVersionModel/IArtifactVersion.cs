using System.Collections.Generic;
using System.Net;
using Model.ArtifactVersionModel.Impl;
using Model.ArtifactModel;

namespace Model.ArtifactVersionModel
{
    public interface IArtifactVersion
    {
        List<LockResultInfo> LockArtifactIds(IUser user, List<int> artifactIds, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        List<DiscardResultInfo> Discard(List<int> artifactIds);
    }
}