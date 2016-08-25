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
        /// <param name="shouldGetLockForUpdate">(optional) Pass false if you don't want to get a lock before trying to update the artifact.  Default is true.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        void Save(IUser user = null, bool shouldGetLockForUpdate = true, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

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
        /// Discard changes to an artifact on Blueprint server using NOVA endpoint (not OpenAPI).
        /// (Runs: /svc/shared/artifacts/discard)
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint. If null, attempts to discard changes using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The NovaDiscardArtifactResult list after discard artifact call</returns>
        List<NovaDiscardArtifactResult> NovaDiscard(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets the Version property of an Artifact.
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The historical version of the artifact.</returns>
        int GetVersion(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets diagram content for RapidReview (Storyteller).
        /// (Runs:  svc/components/RapidReview/diagram/{artifactId})
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>Properties and (for graphical artifacts) diagram content.</returns>
        /// <exception cref="ArgumentException">If method called for Artifact different than diagram.</exception>
        RapidReviewDiagram GetDiagramContentForRapidReview(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets UseCase content for RapidReview (Storyteller).
        /// (Runs: svc/components/RapidReview/usecase/{artifactId})
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>Properties and (for graphical artifacts) diagram content.</returns>
        ///<exception cref="ArgumentException">If method called for Artifact different than Usecase.</exception>
        RapidReviewUseCase GetUseCaseContentForRapidReview(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets glossary content for RapidReview (Storyteller).
        /// (Runs: svc/components/RapidReview/glossary/{artifactId})
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>Properties and (for graphical artifacts) diagram content.</returns>
        /// <exception cref="ArgumentException">If method called for Artifact different than Glossary.</exception>
        RapidReviewGlossary GetGlossaryContentForRapidReview(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets artifact info.
        /// (Runs: svc/components/storyteller/artifactInfo/{artifactId})
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>Artifact info is used by other metod to determine type of artifact</returns>
        ArtifactInfo GetArtifactInfo(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets properties for RapidReview (Storyteller).
        /// (Runs: svc/components/RapidReview/artifacts/properties)
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>Properties and (for graphical artifacts) diagram content.</returns>
        RapidReviewProperties GetPropertiesForRapidReview(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Lock this Artifact.
        /// (Runs:  /svc/shared/artifacts/lock  with artifact ID in the request body)
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to save using the credentials
        ///     of the user that created the artifact.</param>
        /// <param name="expectedLockResult">(optional) The expected LockResult returned in the JSON body.  This is only checked if StatusCode = 200.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The artifact lock result information</returns>
        LockResultInfo Lock(IUser user = null, LockResult expectedLockResult = LockResult.Success, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Publish the artifact on Blueprint server.
        /// (Runs: /svc/shared/artifacts/publish)
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to save using the credentials
        /// of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        NovaPublishArtifactResult NovaPublish(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Creates new discussion for the specified artifact/subartifact using Raptor REST API.
        /// (Runs: /svc/components/RapidReview/artifacts/{artifactId}/discussions)
        /// </summary>
        /// <param name="discussionsText">text for the new discussion</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>RaptorDiscussion for artifact/subartifact</returns>
        IRaptorComment PostRaptorDiscussions(string discussionsText,
            IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Updates the specified comment.
        /// (Runs: PATCH /svc/components/RapidReview/artifacts/{itemId}/discussions/{discussionId})
        /// </summary>
        /// <param name="discussionText">new text for discussion</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="commentToUpdate">comment to update</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>updated RaptorDiscussion</returns>
        IRaptorComment UpdateRaptorDiscussions(string discussionText,
            IUser user, IRaptorComment commentToUpdate,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Deletes the specified comment using Raptor REST API.
        /// (Runs: DELETE /svc/components/RapidReview/artifacts/{artifactId}/deletethread/{commentToDeleteId})
        /// </summary>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="commentToDelete">comment to delete</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>updated RaptorDiscussion</returns>
        string DeleteRaptorDiscussion(IUser user, IRaptorComment commentToDelete,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Adds attachment to the specified artifact.
        /// </summary>
        /// <param name="file">File to attach</param>
        /// <param name="user">The user to authenticate with</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only '201' is expected.</param>
        /// <returns>OpenApiAttachment object</returns>
        OpenApiAttachment AddArtifactAttachment(IFile file, IUser user, 
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Adds attachment to the specified subartifact.
        /// </summary>
        /// <param name="subArtifactId">Id of subartifact to attach file</param>
        /// <param name="file">File to attach</param>
        /// <param name="user">The user to authenticate with</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only '201' is expected.</param>
        /// <returns>OpenApiAttachment object</returns>
        OpenApiAttachment AddSubArtifactAttachment(int subArtifactId,
            IFile file, IUser user, List<HttpStatusCode> expectedStatusCodes = null);
    }
}
