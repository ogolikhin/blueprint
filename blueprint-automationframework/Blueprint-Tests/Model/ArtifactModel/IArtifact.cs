using System;
using Model.ArtifactModel.Impl;
using System.Collections.Generic;
using System.Net;
using Model.NovaModel.Components.RapidReview;

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
        void Save(IUser user = null, bool shouldGetLockForUpdate = true, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Discard changes to an artifact on Blueprint server.
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.  If null, attempts to discard changes using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>The DiscardedArtifactResult list after discard artifact call</returns>
        List<DiscardArtifactResult> Discard(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Discard changes to an artifact on Blueprint server using NOVA endpoint (not OpenAPI).
        /// (Runs: 'POST /svc/shared/artifacts/discard')
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.  If null, attempts to discard changes using the credentials
        ///     of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>The NovaDiscardArtifactResult list after discard artifact call.</returns>
        List<NovaDiscardArtifactResult> NovaDiscard(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets the Version property of an Artifact.
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        /// of the user that created the artifact. </param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>The historical version of the artifact.</returns>
        int GetVersion(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets Diagram content for RapidReview (Storyteller).
        /// (Runs:  'GET /svc/components/RapidReview/diagram/{artifactId}')
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint.  If null, attempts to get the version using the credentials
        ///     of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>Properties and (for graphical artifacts) Diagram content.</returns>
        /// <exception cref="ArgumentException">If method called for Artifact different than diagram.</exception>
        RapidReviewDiagram GetRapidReviewDiagramContent(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets UseCase content for RapidReview (Storyteller).
        /// (Runs: svc/components/RapidReview/usecase/{artifactId})
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        ///     of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>Properties and UseCase content.</returns>
        ///<exception cref="ArgumentException">If method called for Artifact different than Usecase.</exception>
        UseCase GetRapidReviewUseCaseContent(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets Glossary content for RapidReview (Storyteller).
        /// (Runs: svc/components/RapidReview/glossary/{artifactId})
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        ///     of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>Properties and Glossary content.</returns>
        /// <exception cref="ArgumentException">If method called for Artifact different than Glossary.</exception>
        RapidReviewGlossary GetRapidReviewGlossaryContent(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets artifact properties for RapidReview (Storyteller).
        /// (Runs: 'GET svc/components/RapidReview/artifacts/properties' with the artifactId in the body.)
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        ///     of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>Properties of this artifact.</returns>
        RapidReviewProperties GetRapidReviewArtifactProperties(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets artifact info.
        /// (Runs: svc/components/storyteller/artifactInfo/{artifactId})
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to get the version using the credentials
        ///     of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>Artifact info is used by other method to determine type of artifact</returns>
        ArtifactInfo GetArtifactInfo(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Lock this Artifact.
        /// (Runs:  'POST /svc/shared/artifacts/lock'  with artifact ID in the request body)
        /// </summary>
        /// <param name="user">The user locking the artifact.</param>
        /// <param name="expectedLockResult">(optional) The expected LockResult returned in the JSON body.  This is only checked if StatusCode = 200.
        ///     If null, only Success is expected.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>The LockResultInfo for the locked artifact.</returns>
        LockResultInfo Lock(IUser user = null,
            LockResult expectedLockResult = LockResult.Success,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Publish the artifact on Blueprint server.  This is only used by Storyteller.
        /// (Runs: 'POST /svc/shared/artifacts/publish')
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint.  If null, attempts to save using the credentials
        ///     of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>Results of Publish operation.</returns>
        NovaPublishArtifactResult StorytellerPublish(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Creates new discussion for the specified artifact/subartifact using Raptor REST API.
        /// (Runs: /svc/components/RapidReview/artifacts/{artifactId}/discussions)
        /// </summary>
        /// <param name="comment">The comment for the new discussion.</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>RaptorDiscussion for artifact/subartifact</returns>
        IRaptorDiscussion PostRapidReviewArtifactDiscussion(string comment,
            IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Updates the specified discussion.
        /// (Runs: PATCH /svc/components/RapidReview/artifacts/{itemId}/discussions/{discussionId})
        /// </summary>
        /// <param name="comment">The new comment for discussion.</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="discussionToUpdate">The discussion to update.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>updated RaptorDiscussion</returns>
        IRaptorDiscussion UpdateRaptorDiscussion(RaptorComment comment,
            IUser user, IRaptorDiscussion discussionToUpdate,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Deletes the specified discussion using Raptor REST API.
        /// (Runs: DELETE /svc/components/RapidReview/artifacts/{artifactId}/deletethread/{commentToDeleteId})
        /// </summary>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="discussionToDelete">The discussion to delete.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>A success or failure message.</returns>
        string DeleteRaptorDiscussion(IUser user, IRaptorDiscussion discussionToDelete,
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
