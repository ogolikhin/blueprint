using Model.NavigationModel.Impl;
using Model.OpenApiModel;
using System.Collections.Generic;
using System.Net;

namespace Model.NavigationModel
{
    public interface INavigation
    {
        #region Properties
        
        /// <summary>
        /// List of artifacts in the Blueprint main experience.
        /// </summary>
        List<IOpenApiArtifact> Artifacts { get; }

        /// <summary>
        /// List of artifact references which contains information for breadcrumb artifact navigation.
        /// </summary>
        List<ArtifactReference> ArtifactReferenceList { get; }
        
        #endregion Properties

        #region Methods
        
        /// <summary>
        /// Get ArtifactReference list which is used to represent breadcrumb navigation
        /// </summary>
        /// <param name="user">The user credentials for the request to publish a process</param>
        /// <param name="artifacts">The list of artifacts used for breadcrumb navigation</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The List of ArtifactReferences after the get navigation call</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<IArtifactReference> GetNavigation(IUser user, List<IOpenApiArtifact> artifacts, List<HttpStatusCode> expectedStatusCodes = null, 
            bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Delete the artifact used for breadcrumb navigation
        /// </summary>
        /// <param name="artifact">The artifact used for breadcrumb navigation</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The DeleteArtifactResult after the detelte artifact call</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<IDeleteArtifactResult> DeleteNavigationArtifact(IOpenApiArtifact artifact, List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false, bool deleteChildren = false);
        
        #endregion Methods
    }
}
