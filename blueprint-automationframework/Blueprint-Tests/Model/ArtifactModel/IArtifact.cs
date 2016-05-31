using System;
using System.Collections.Generic;
using System.Net;
using Model.ArtifactModel.Impl;

namespace Model.ArtifactModel
{
    public interface IArtifact : IArtifactBase
    {
        /// <summary>
        /// Save the artifact on Blueprint server.
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to save using the credentials
        /// of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        void Save(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Discard changes to an artifact on Blueprint server.
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint. If null, attempts to discard changes using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The DiscardedArtifactResult list after discard artifact call</returns>
        List<DiscardArtifactResult> Discard(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Discard changes to an artifact on Blueprint server using NOVA endpoint(not OpenAPI).
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint. If null, attempts to discard changes using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The NovaDiscardArtifactResult list after discard artifact call</returns>
        List<NovaDiscardArtifactResult> NovaDiscard(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets the Version property of an Artifact
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The historical version of the artifact.</returns>
        int GetVersion(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets diagram content for RapidReview (Storyteller)
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>Properties and (for graphical artifacts) diagram content.</returns>
        /// <exception cref="ArgumentException">If method called for Artifact different than diagram.</exception>
        RapidReviewDiagram GetDiagramContentForRapidReview(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets UseCase content for RapidReview (Storyteller)
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>Properties and (for graphical artifacts) diagram content.</returns>
        ///<exception cref="ArgumentException">If method called for Artifact different than Usecase.</exception>
        RapidReviewUseCase GetUseCaseContentForRapidReview(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets glossary content for RapidReview (Storyteller)
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>Properties and (for graphical artifacts) diagram content.</returns>
        /// <exception cref="ArgumentException">If method called for Artifact different than Glossary.</exception>
        RapidReviewGlossary GetGlossaryContentForRapidReview(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets artifact info
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>Artifact info is used by other metod to determine type of artifact</returns>
        ArtifactInfo GetArtifactInfo(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets properties for RapidReview (Storyteller)
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>Properties and (for graphical artifacts) diagram content.</returns>
        RapidReviewProperties GetPropertiesForRapidReview(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Lock an Artifact
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to save using the credentials
        /// of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The artifact lock result information</returns>
        LockResultInfo Lock(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Updates artifact name with a random string
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to save using the credentials
        /// of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        void UpdateArtifactName(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Publish the artifact on Blueprint server.
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to save using the credentials
        /// of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        NovaPublishArtifactResult NovaPublish(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);
    }
}
