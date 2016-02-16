using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Model
{
    public interface IArtifactStore
    {
        /// <summary>
        /// Adds the specified artifact to the ArtifactStore.
        /// </summary>
        /// <param name="artifact">The artifact to add.</param>
        /// <param name="user">The user to authenticate to the ArtifactStore.</param>
        /// <param name="expectedStatusCodes">A list of expected status codes.  By default, only '201 Success' is expected.</param>
        /// <returns>The artifact that was created (including the artifact ID that ArtifactStore gave it).</returns>
        /// <exception cref="WebException">A WebException sub-class if ArtifactStore returned an unexpected HTTP status code.</exception>
        IArtifact AddArtifact(IArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null);
        
        /// <summary>
        /// Deletes the specified artifact from the ArtifactStore.
        /// </summary>
        /// <param name="artifact"> Artifact to be deleted</param>
        /// <param name="user">The user to authenticate to the ArtifactStore.</param>
        /// <param name="expectedStatusCodes">A list of expected status codes.</param>
        /// <returns></returns>
        IArtifactResult DeleteArtifact(IArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null);
    }
}
