﻿using System;
using System.Collections.Generic;
using System.Net;
using Model.ArtifactModel.Impl;
using Model.StorytellerModel;

namespace Model
{
    public interface ISvcComponents
    {
        #region FileStore methods

        /// <summary>
        /// Upload a File.
        /// </summary>
        /// <param name="user">The user credentials for the request to upload the file.</param>
        /// <param name="file">The file to upload.</param>
        /// <param name="expireDate">(optional) Expected expire date for the file.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.</param>
        /// <returns>The REST response content of the upload file request.</returns>
        string UploadFile(
            IUser user,
            IFile file,
            DateTime? expireDate = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        #endregion FileStore methods

        #region RapidReview methods

        /// <summary>
        /// Gets Diagram content for RapidReview (Storyteller).
        /// (Runs:  'GET /svc/components/RapidReview/diagram/{artifactId}')
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="artifactId">The ID of the Diagram artifact whose contents you want to get.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>Properties and (for graphical artifacts) Diagram content.</returns>
        RapidReviewDiagram GetRapidReviewDiagramContent(
            IUser user,
            int artifactId,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets Glossary content for RapidReview (Storyteller).
        /// (Runs:  'GET /svc/components/RapidReview/glossary/{artifactId}')
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="artifactId">The ID of the Glossary artifact whose contents you want to get.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>Properties and Glossary content.</returns>
        RapidReviewGlossary GetRapidReviewGlossaryContent(
            IUser user,
            int artifactId,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets Use Case content for RapidReview (Storyteller).
        /// (Runs:  'GET /svc/components/RapidReview/glossary/{artifactId}')
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="artifactId">The ID of the Use Case artifact whose contents you want to get.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>Properties and Use Case content.</returns>
        RapidReviewUseCase GetRapidReviewUseCaseContent(
            IUser user,
            int artifactId,
            List<HttpStatusCode> expectedStatusCodes = null);

        #endregion RapidReview methods

        #region  Storyteller methods

        /// <summary>
        /// Gets artifact info.
        /// (Runs: 'GET svc/components/storyteller/artifactInfo/{artifactId}')
        /// </summary>
        /// <param name="artifactId">The artifact id</param>
        /// <param name="user">(optional) The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>Artifact info is used by other metod to determine type of artifact</returns>
        ArtifactInfo GetArtifactInfo(
            int artifactId,
            IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Get a Process
        /// 
        /// (Runs: 'GET svc/components/storyteller/processes/{artifactId})
        /// </summary>
        /// <param name="artifactId">Id of the process artifact from which the process is obtained</param>
        /// <param name="user">(optional)The user credentials for the request to get a process</param>
        /// <param name="versionIndex">(optional) The version of the process artifact</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The requested process object</returns>
        IProcess GetProcess(
            int artifactId,
            IUser user = null,
            int? versionIndex = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Get a List of Processes for the specified Project Id
        /// 
        /// (Runs: 'GET svc/components/storyteller/projects/{projectId}/processes/)
        /// </summary>
        /// <param name="projectId">The Id of the project</param>
        /// <param name="user">(optional)The user credentials for the request to get the process list</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The list of process objects</returns>
        IList<IProcess> GetProcesses(int projectId, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Get the User Story Artifact Type for the Project
        /// 
        /// (Runs: 'GET svc/components/storyteller/projects/{projectId}/artifacttypes/userstory')
        /// </summary>
        /// <param name="projectId">The Id of the Project from which the user story artifact type is retrieved</param>
        /// <param name="user">The user credentials for the request to get the user story artifact type</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The user story artifact type</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        OpenApiArtifactType GetUserStoryArtifactType(int projectId, IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Update a Process
        /// 
        /// (Runs: 'POST svc/components/storyteller/processes/{artifactId})
        /// </summary>
        /// <param name="process">The process to update</param>
        /// <param name="user">(optional) The user credentials for the request to update a process</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The returned process result</returns>
        ProcessUpdateResult UpdateProcess(
            IProcess process,
            IUser user = null, 
            List<HttpStatusCode> expectedStatusCodes = null);

        #endregion Storyteller methods
    }
}