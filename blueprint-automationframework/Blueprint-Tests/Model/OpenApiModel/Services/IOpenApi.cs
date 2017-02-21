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
        /// Create a user with specified user properties.
        /// (Runs:  'POST /api/v1/users')
        /// </summary>
        /// <param name="userToAuthenticate">A user that has permission to create users.</param>
        /// <param name="userToCreate">User to create.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '201 Created' is expected.</param>
        /// <returns>User that was created.</returns>
        UserDataModel CreateUser(
            IUser userToAuthenticate,
            UserDataModel userToCreate,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Delete a list of users with specific usernames.
        /// (Runs:  'DELETE /api/v1/users')
        /// </summary>
        /// <param name="userToAuthenticate">A user that has permission to delete users.</param>
        /// <param name="usernamesToDelete">Usernames of users to delete.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>List of usernames with their error codes and messages that was deleted.</returns>
        UserDeleteResultCollection DeleteUsers(
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

        /// <summary>
        /// Updates a list of users with specific usernames.
        /// (Runs:  'PATCH /api/v1/users')
        /// </summary>
        /// <param name="userToAuthenticate">A user that has permission to update users.</param>
        /// <param name="usersToUpdate">Users to update (only Type, Username and the properties being updated are required).</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>List of usernames with their error codes and messages.</returns>
        UserDeleteResultCollection UpdateUsers(
            IUser userToAuthenticate,
            List<UserDataModel> usersToUpdate,
            List<HttpStatusCode> expectedStatusCodes = null);

        #endregion User methods
    }
}
