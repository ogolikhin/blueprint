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
        /// Gets diagram content for RapidReview (Storyteller).
        /// (Runs:  'GET /svc/components/RapidReview/diagram/{artifactId}')
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="artifactId">The ID of the diagram artifact whose contents you want to get.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>Properties and (for graphical artifacts) diagram content.</returns>
        RapidReviewDiagram GetRapidReviewDiagramContent(
            IUser user,
            int artifactId,
            List<HttpStatusCode> expectedStatusCodes = null);

        #endregion RapidReview methods

        #region  Storyteller methods

        /// <summary>
        /// Gets artifact info.
        /// (Runs: 'GET svc/components/storyteller/artifactInfo/{artifactId}')
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="artifactId">The artifact id</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>Artifact info is used by other metod to determine type of artifact</returns>
        ArtifactInfo GetArtifactInfo(
            IUser user,
            int artifactId,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Get a Process
        /// </summary>
        /// <param name="user">The user credentials for the request to get a process</param>
        /// <param name="artifactId">Id of the process artifact from which the process is obtained</param>
        /// <param name="versionIndex">(optional) The version of the process artifact</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The requested process object</returns>
        IProcess GetProcess(
            IUser user, 
            int artifactId, 
            int? versionIndex = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Update a Process
        /// </summary>
        /// <param name="user">The user credentials for the request to update a process</param>
        /// <param name="process">The process to update</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The returned process result</returns>
        ProcessUpdateResult UpdateProcess(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null);

        #endregion Storyteller methods
    }
}
