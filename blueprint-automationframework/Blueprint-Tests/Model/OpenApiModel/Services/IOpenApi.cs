using System.Collections.Generic;
using System.Net;
using Model.Impl;
using Model.JobModel;
using Model.OpenApiModel.UserModel;
using Model.OpenApiModel.UserModel.Results;

namespace Model.OpenApiModel.Services
{
    public interface IOpenApi
    {
        string Address { get; }

        #region ALM Jobs methods

        /// <summary>
        /// Add ALM ChangeSummary Job using OpenAPI.
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="project">The project to have the ALM ChangeSummary job.</param>
        /// <param name="baselineOrReviewId">The baseline or review artifact ID.</param>
        /// <param name="almTarget">The ALM target.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <returns>OpenAPIJob that contains information for ALM ChangeSummary Job.</returns>
        IOpenAPIJob AddAlmChangeSummaryJob(
            IUser user,
            IProject project,
            int baselineOrReviewId,
            IAlmTarget almTarget,
            List<HttpStatusCode> expectedStatusCodes = null);

        #endregion ALM Jobs methods

        #region User methods

        /// <summary>
        /// Create users with specified properties.
        /// (Runs:  'POST /api/v1/users')
        /// </summary>
        /// <param name="userToAuthenticate">A user that has permission to create users.</param>
        /// <param name="usersToCreate">List of users to create.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '201 Created' is expected.</param>
        /// <returns>Collection of users which were created with returned http code and message.</returns>
        UserCallResultCollection CreateUser(
            IUser userToAuthenticate,
            List<OpenApiUser> usersToCreate,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Delete a user with specific username.
        /// (Runs:  'DELETE /api/v1/users/delete')
        /// </summary>
        /// <param name="userToAuthenticate">A user that has permission to delete users.</param>
        /// <param name="usernamesToDelete">Usernames of users to delete.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>List of usernames with their error codes and messages that was created together with global HTTP code.</returns>
        UserCallResultCollection DeleteUser(
            IUser userToAuthenticate,
            List<string> usernamesToDelete,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Get a user with the specified ID.
        /// (Runs:  'GET /api/v1/users/{id}')
        /// </summary>
        /// <param name="userToAuthenticate">A user that has permission to get users.</param>
        /// <param name="userId">ID of the user to get.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>The details for the specified user.</returns>
        GetUserResult GetUser(
            IUser userToAuthenticate,
            int userId,
            List<HttpStatusCode> expectedStatusCodes = null);

        #endregion User methods
    }
}
