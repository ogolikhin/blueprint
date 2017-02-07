﻿using System;
using System.Collections.Generic;
using System.Net;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Impl;

namespace Model.StorytellerModel
{
    public interface IStoryteller : IDisposable
    {
        /// <summary>
        /// List of created artifacts.
        /// </summary>
        List<IArtifact> Artifacts { get; }

        /// <summary>
        /// List of created Nova Process artifacts.
        /// </summary>
        List<NovaProcess> NovaProcesses { get; }

        /// <summary>
        /// Create and Save a Process artifact
        /// </summary>
        /// <param name="project">The project where the process artifact is to be added</param>
        /// <param name="user">The user credentials for the request to create the process artifact</param>
        /// <param name="expectedStatusCodes">(optional) Expected status code for this call. By default, only '201 Success' is expected.</param>
        /// 
        /// <returns>The saved artifact object</returns>
        IArtifact CreateAndSaveProcessArtifact(IProject project, IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Create and Save Multiple Process Artifacts
        /// </summary>
        /// <param name="project">The project where the process artifacts are to be added</param>
        /// <param name="user">The user credentials for the request to create the process artifacts</param>
        /// <param name="numberOfArtifacts">The number of process artifacts to create</param>
        /// <returns>The list of the saved artifact objects</returns>
        List<IArtifact> CreateAndSaveProcessArtifacts(IProject project, IUser user, int numberOfArtifacts);

        /// <summary>
        /// Create and Publish a single Process Artifact
        /// </summary>
        /// <param name="project">The project where the process artifact is to be added</param>
        /// <param name="user">The user credentials for the request to create the process artifacts</param>
        /// <returns>the published artifact object</returns>
        IArtifact CreateAndPublishProcessArtifact(IProject project, IUser user);

        /// <summary>
        /// Create and Publish Multiple Process Artifacts
        /// </summary>
        /// <param name="project">The project where the process artifacts are to be added</param>
        /// <param name="user">The user credentials for the request to create the process artifacts</param>
        /// <param name="numberOfArtifacts">The number of process artifacts to create</param>
        /// <returns>The list of the published artifact objects</returns>
        List<IArtifact> CreateAndPublishProcessArtifacts(IProject project, IUser user, int numberOfArtifacts);

        /// <summary>
        /// Create and Save a Nova Process artifact
        /// </summary>
        /// <param name="project">The project where the Nova process artifact is to be added</param>
        /// <param name="user">The user credentials for the request to create the Nova process artifact</param>
        /// <param name="parentId">(optional) The ID of the parent of the Nova process artifact to be created.</param>
        /// <param name="orderIndex">(optional) The Order Index to assign to the new Nova Process artifact.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status code for this call. By default, only '201 Success' is expected.</param>
        /// <returns>The saved Nova process artifact object</returns>
        NovaProcess CreateAndSaveNovaProcessArtifact(IProject project, 
            IUser user, 
            int? parentId = null,
            double? orderIndex = null, 
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Create and Save Multiple Nova Process Artifacts
        /// </summary>
        /// <param name="project">The project where the Nova process artifacts are to be added</param>
        /// <param name="user">The user credentials for the request to create the Nova process artifacts</param>
        /// <param name="numberOfArtifacts">The number of Nova process artifacts to create</param>
        /// <param name="parentId">(optional) The ID of the parent of the Nova process artifact to be created.</param>
        /// <param name="orderIndex">(optional) The Order Index to assign to the new Nova Process artifact.</param>
        /// <returns>The list of the saved Nova process artifact objects</returns>
        List<NovaProcess> CreateAndSaveNovaProcessArtifacts(IProject project, 
            IUser user, 
            int numberOfArtifacts,
            int? parentId = null,
            double? orderIndex = null);

        /// <summary>
        /// Create and Publish a single Process Artifact
        /// </summary>
        /// <param name="project">The project where the Nova process artifact is to be added</param>
        /// <param name="user">The user credentials for the request to create the Nova process artifacts</param>
        /// <returns>the published artifact object</returns>
        NovaProcess CreateAndPublishNovaProcessArtifact(IProject project, IUser user);

        /// <summary>
        /// Create and Publish Multiple Nova Process Artifacts
        /// </summary>
        /// <param name="project">The project where the Nova process artifacts are to be added</param>
        /// <param name="user">The user credentials for the request to create the Nova process artifacts</param>
        /// <param name="numberOfArtifacts">The number of Nova process artifacts to create</param>
        /// <param name="parentId">(optional) The ID of the parent of the Nova process artifact to be created.</param>
        /// <param name="orderIndex">(optional) The Order Index to assign to the new Nova Process artifact.</param>
        /// <returns>The list of the published Nova process artifact objects</returns>
        List<NovaProcess> CreateAndPublishNovaProcessArtifacts(IProject project, 
            IUser user, 
            int numberOfArtifacts,
            int? parentId = null,
            double? orderIndex = null);

        /// <summary>
        /// Generate or Update User Stories for the Process Artifact.
        /// </summary>
        /// <param name="user">The user credentials for the request to generate the user stories</param>
        /// <param name="process">The process from which user stories are generated.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <param name="shouldDeleteChildren">(optional) Flag to skip deleting process children (Default: true)</param>
        /// <returns>The list of user stories that were generated or updated</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<IStorytellerUserStory> GenerateUserStories(IUser user,
            IProcess process,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false,
            bool shouldDeleteChildren = true);

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
        /// Get a Nova Process (Storyteller 2.1+)
        /// svc/bpartifactstore/process/{0}
        /// </summary>
        /// <param name="user">The user credentials for the request to get a process</param>
        /// <param name="artifactId">Id of the process artifact from which the process is obtained</param>
        /// <param name="versionIndex">(optional) The version of the process artifact</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request. By default only 200 OK is expected.</param>
        /// <returns>The requested Nova process object</returns>
        NovaProcess GetNovaProcess(IUser user, int artifactId, int? versionIndex = null, List<HttpStatusCode> expectedStatusCodes = null);

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
        /// Get the User Story Artifact Type for the Project
        /// </summary>
        /// <param name="user">The user credentials for the request to get the user story artifact type</param>
        /// <param name="projectId">The Id of the Project from which the user story artifact type is retrieved</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The user story artifact type</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        OpenApiArtifactType GetUserStoryArtifactType(IUser user, int projectId, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Upload a File
        /// </summary>
        /// <param name="user">The user credentials for the request to upload the file</param>
        /// <param name="file">The file to upload</param>
        /// <param name="expireDate">(optional) Expected expire date for the file</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The REST response content of the upload file request</returns>
        string UploadFile(IUser user, IFile file, DateTime? expireDate = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Update a Process
        /// </summary>
        /// <param name="user">The user credentials for the request to update a process</param>
        /// <param name="process">The process to update</param>
        /// <param name="lockArtifactBeforeUpdate">(optional) Flag indicating whether or not the process artifact should be locked before update (Default: true)</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The updated process</returns>
        IProcess UpdateProcess(IUser user, IProcess process, bool lockArtifactBeforeUpdate = true, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Update a Nova Process (Storyteller 2.1+)
        /// svc/bpartifactstore/processupdate/{0}
        /// </summary>
        /// <param name="user">The user credentials for the request to update a Nova process</param>
        /// <param name="novaProcess">The Nova process to update</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request. By default only 200 OK is expected.</param>
        /// <returns>The updated Nova process</returns>
        NovaProcess UpdateNovaProcess(IUser user, NovaProcess novaProcess, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Update a Process but only return the JSON response as a string. (Used only when a response other than a process object
        /// is expected - i.e. when testing a negative case where an error message is expected rather than a process object)
        /// </summary>
        /// <param name="user">The user credentials for the request to update a process</param>
        /// <param name="process">The process to update</param>
        /// <param name="lockArtifactBeforeUpdate">(optional) Flag indicating whether or not the process artifact should be locked before update (Default: true)</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The REST response content of the update process request</returns>
        string UpdateProcessReturnResponseOnly(IUser user, IProcess process, bool lockArtifactBeforeUpdate = true, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Publish a Process Artifact (Used when publishing a single process artifact)
        /// </summary>
        /// <param name="user">The user credentials for the request to publish a process</param>
        /// <param name="process">The process to publish</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The REST response content of the publish process request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        string PublishProcess(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Discard changes to a process artifact
        /// </summary>
        /// <param name="artifact">The artifact with changes to be discarded</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The List of DiscardArtifactResult after the call</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<DiscardArtifactResult> DiscardProcessArtifact(IArtifact artifact, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Delete a process artifact
        /// </summary>
        /// <param name="artifact">The artifact to be deleted</param>
        /// <param name="deleteChildren">(optional) Specifies whether or not to also delete all child artifacts of the specified artifact</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The List of DeleteArtifactResult after the call</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<OpenApiDeleteArtifactResult> DeleteProcessArtifact(IArtifact artifact, bool? deleteChildren = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Delete a Nova process artifact
        /// svc/bpartifactstore/artifacts/{0}
        /// </summary>
        /// <param name="user">The user credentials for the request to delete a Nova process</param>
        /// <param name="novaProcess">The Nova process artifact to delete</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The list of Nova Artifacts that were deleted.</returns>
        List<NovaArtifact> DeleteNovaProcessArtifact(IUser user, NovaProcess novaProcess, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Retrieves the Storyteller limit from the ApplicationSettings table
        /// </summary>
        /// <returns>Returns shape limit for storyteller</returns>
        int GetStorytellerShapeLimitFromDb { get; }

        /// <summary>
        /// Returns URL of the Blueprint server
        /// </summary>
        string Address { get; }

    }
}
