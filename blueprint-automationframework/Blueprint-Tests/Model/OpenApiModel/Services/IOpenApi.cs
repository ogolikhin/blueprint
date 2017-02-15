using System.Collections.Generic;
using System.Net;
using Model.Impl;
using Model.JobModel;

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
        /// (Runs:  'POST /api/v1/users/create')
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
        /// Delete a user with specific username.
        /// (Runs:  'DELETE /api/v1/users/delete')
        /// </summary>
        /// <param name="userToAuthenticate">A user that has permission to delete users.</param>
        /// <param name="usernamesToDelete">Usernames of users to delete.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>List of usernames with their error codes and messages that was created together with global HTTP code.</returns>
        DeleteResultSet DeleteUser(
            IUser userToAuthenticate,
            List<string> usernamesToDelete,
            List<HttpStatusCode> expectedStatusCodes = null);

        #endregion User methods
    }
}
