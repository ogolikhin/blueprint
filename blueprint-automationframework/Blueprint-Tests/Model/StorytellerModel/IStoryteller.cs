using System;
using System.Collections.Generic;
using System.Net;
using Model.OpenApiModel;

namespace Model.StorytellerModel
{
    public interface IStoryteller
    {
        /// <summary>
        /// List of created artifacts.
        /// </summary>
        List<IOpenApiArtifact> Artifacts { get; }

        /// <summary>
        /// Create a Process artifact
        /// </summary>
        /// <param name="project">The project where the process artifact is to be added</param>
        /// <param name="artifactType">The base artifact type of the process artifact</param>
        /// <param name="user">The user credentials for the request to create the process artifact</param>
        /// <param name="expectedStatusCodes">(optional) Expected status code for this call. By default, only '201 Success' is expected.</param>
        /// <returns>The created artifact object</returns>
        IOpenApiArtifact CreateAndSaveProcessArtifact(IProject project, BaseArtifactType artifactType, IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Create Multiple Process Artifacts
        /// </summary>
        /// <param name="project">The project where the process artifacts are to be added</param>
        /// <param name="user">The user credentials for the request to create the process artifacts</param>
        /// <param name="numberOfArtifacts">The number of process artifacts to create</param>
        /// <returns>The list of the created artifact objects</returns>
        List<IOpenApiArtifact> CreateAndSaveProcessArtifacts(IProject project, IUser user, int numberOfArtifacts);

        /// <summary>
        /// Generate or Update User Stories for the Process Artifact.
        /// </summary>
        /// <param name="user">The user credentials for the request to generate the user stories</param>
        /// <param name="process">The process from which user stories are generated.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The list of user stories that were generated or updated</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<IStorytellerUserStory> GenerateUserStories(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Get a Process
        /// </summary>
        /// <param name="user">The user credentials for the request to get a process</param>
        /// <param name="artifactId">Id of the process artifact from which the process is obtained</param>
        /// <param name="versionIndex">(optional) The version of the process artifact</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The requested process object</returns>
        IProcess GetProcess(IUser user, int artifactId, int? versionIndex = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Get a List of Processes for the specified Project Id
        /// Runs /projects/id/processes
        /// </summary>
        /// <param name="user">The user credentials for the request to get the process list</param>
        /// <param name="projectId">The Id of the project</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The list of process objects</returns>
        IList<IProcess> GetProcesses(IUser user, int projectId, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Get the Id of the Process Artifact Type from the specified Project
        /// </summary>
        /// <param name="user">The user credentials for the request to get the artifact type Id</param>
        /// <param name="project">The project from which the artifact type is retrieved</param>
        /// <returns>The Id of process artifact type</returns>
        int GetProcessArtifactTypeId(IUser user, IProject project);

        /// <summary>
        /// Get a Process Artifact from a Breadcrumb Trail
        /// </summary>
        /// <param name="user">The user credentials for the request to get the process artifact</param>
        /// <param name="artifactIds">The Ids in the breadcrumb trail.  The last Id is the Id of the Process being retrieved.</param>
        /// <param name="versionIndex">(optional) The version of the process artifact</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The requested process artifact</returns>
        IProcess GetProcessWithBreadcrumb(IUser user, List<int> artifactIds, int? versionIndex = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Get the User Story Artifact Type for the Project
        /// </summary>
        /// <param name="user">The user credentials for the request to get the user story artifact type</param>
        /// <param name="projectId">The Id of the Project from which the user story artifact type is retrieved</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The user story artifact type</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        IArtifactType GetUserStoryArtifactType(IUser user, int projectId, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Upload a File
        /// </summary>
        /// <param name="user">The user credentials for the request to upload the file</param>
        /// <param name="file">The file to upload</param>
        /// <param name="expireDate">(optional) Expected expire date for the file</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The REST response content of the upload file request</returns>
        string UploadFile(IUser user, IFile file, DateTime? expireDate = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Update a Process
        /// </summary>
        /// <param name="user">The user credentials for the request to update a process</param>
        /// <param name="process">The process to update</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The updated process</returns>
        IProcess UpdateProcess(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Update a Process but only return the JSON response as a string. (Used only when a response other than a process object
        /// is expected - i.e. when testing a negative case where an error message is expected rather than a process object)
        /// </summary>
        /// <param name="user">The user credentials for the request to update a process</param>
        /// <param name="process">The process to update</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The REST response content of the update process request</returns>
        string UpdateProcessReturnResponseOnly(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Publish a Process Artifact (Used when publishing a single process artifact)
        /// </summary>
        /// <param name="user">The user credentials for the request to publish a process</param>
        /// <param name="process">The process to publish</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The REST response content of the publish process request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        string PublishProcessArtifact(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Publish Process Artifact(s) (Used when publishing a single process artifact OR a list of process artifacts)
        /// </summary>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="shouldKeepLock">(optional) Boolean parameter which defines whether or not to keep the lock after publishing the artfacts</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The list of PublishArtifactResult objects created by the publish process artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<IPublishArtifactResult> PublishProcessArtifacts(IUser user, bool shouldKeepLock = false, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Delete a process artifact
        /// </summary>
        /// <param name="artifact">The artifact to be deleted</param>
        /// <param name="user">The user credentials for the request to delete the artifact</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="deleteChildren">(optional) Specifies whether or not to also delete all child artifacts of the specified artifact</param>
        /// <returns>The List of DeleteArtifactResult after the call</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<IDeleteArtifactResult> DeleteProcessArtifact(IOpenApiArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null, bool deleteChildren = false);

        /// <summary>
        /// Returns URL of the Blueprint server
        /// </summary>
        string Address { get; }
    }
}
