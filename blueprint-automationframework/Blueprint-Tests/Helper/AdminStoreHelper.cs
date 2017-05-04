using Common;
using Model;
using Model.ArtifactModel;
using Model.Impl;
using Model.JobModel;
using Model.JobModel.Impl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Model.Common.Enums;
using Utilities;
using Utilities.Factories;

namespace Helper
{
    public static class AdminStoreHelper
    {
        #region Project Management

        #endregion Project Management

        #region Custom Asserts

        /// <summary>
        /// Assert that project1 and project2 are the same.
        /// </summary>
        /// <param name="expectedProject">IProject representing a project.</param>
        /// <param name="actualProject">IProject representing a project.</param>
        /// <param name="convertDescriptionsToPlainText">Indicator if plain text conversion is required for both projects' description values.
        /// By Default, this is set to false.</param>
        public static void AssertAreEqual(IProject expectedProject, IProject actualProject, bool convertDescriptionsToPlainText = false)
        {
            ThrowIf.ArgumentNull(expectedProject, nameof(expectedProject));
            ThrowIf.ArgumentNull(actualProject, nameof(actualProject));

            
            if (!string.IsNullOrEmpty(actualProject.Description))
            {
                if (convertDescriptionsToPlainText)
                {
                    var expectedProjectDescription = StringUtilities.ConvertHtmlToText(expectedProject.Description);
                    var actualProjectDescription = StringUtilities.ConvertHtmlToText(actualProject.Description);

                    Assert.AreEqual(expectedProjectDescription, actualProjectDescription,
                        "Project Description '{0}' was expected but '{1}' was returned.",
                        expectedProjectDescription, actualProjectDescription);
                } else
                {
                    Assert.AreEqual(expectedProject.Description, actualProject.Description,
                        "Project Description '{0}' was expected but {1} was returned.",
                        expectedProject.Description, actualProject.Description);
                }
            }

            Assert.AreEqual(expectedProject.Id, actualProject.Id, "Project Id {0} was expected but {1} was returned.",
                expectedProject.Id, actualProject.Id);

            Assert.AreEqual(expectedProject.Name, actualProject.Name, "Project Name '{0}' was expected but '{1}' was returned.",
                expectedProject.Name, actualProject.Name);
        }

        /// <summary>
        /// Assert that InstanceProject returned from Get ProjectById is the same as the project
        /// </summary>
        /// <param name="helper">A TestHelper instance.</param>
        /// <param name="expectedProject">the project used to compare with returned instanceProject</param>
        /// <param name="actualInstanceProject">instanceProject that is returned from Get ProjectById</param>
        public static void AssertAreEqual(TestHelper helper, IProject expectedProject, InstanceProject actualInstanceProject)
        {
            ThrowIf.ArgumentNull(helper, nameof(helper));
            ThrowIf.ArgumentNull(expectedProject, nameof(expectedProject));

            ThrowIf.ArgumentNull(actualInstanceProject, nameof(InstanceProject));

            AssertAreEqual(expectedProject, (IProject)actualInstanceProject, convertDescriptionsToPlainText: true);

            Assert.IsNotNull(actualInstanceProject.ParentFolderId, "{0} should not be null!", nameof(InstanceProject.ParentFolderId));

            Assert.AreEqual(InstanceItemTypeEnum.Project, actualInstanceProject.Type, "Type '{0}' was expected but '{1}' was returned.",
                InstanceItemTypeEnum.Project, actualInstanceProject.Type);

            var expectedPermissionForProject = helper.ProjectRoles.Find(p => p.ProjectId.Equals(expectedProject.Id))?.Permissions;

            Assert.AreEqual(expectedPermissionForProject, actualInstanceProject.Permissions,
                "Permission {0} was expected but {1} is returned.", expectedPermissionForProject, actualInstanceProject.Permissions);
        }

        #endregion Custom Asserts

        #region Job Management

        /// <summary>
        /// Create GenerateProcessTestsJobParameters used for QueueGenerateProcessTestsJob call
        /// </summary>
        /// <param name="project">The target project</param>
        /// <param name="artifacts">Artifact list which will be use for the Process Tests generation job</param>
        /// <returns>GenerateProcessTestsJobParameters which is used for scheduling a job for test generation</returns>
        public static GenerateProcessTestsJobParameters GenerateProcessTestsJobParameters(
            IProject project,
            List<IArtifactBase> artifacts
            )
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));

            var generateProcessTestInfoList = new List<GenerateProcessTestInfo>();

            artifacts.ForEach(a => generateProcessTestInfoList.Add(new GenerateProcessTestInfo(a.Id)));

            var generateProcessTestsJobParameters = new GenerateProcessTestsJobParameters(
                project.Id, project.Name, generateProcessTestInfoList);

            return generateProcessTestsJobParameters;
        }

        #region Custom Asserts

        /// <summary>
        /// Asserts that returned jobResult from the Nova GET job or jobs call match with jobs that are being retrieved.
        /// </summary>
        /// <param name="jobResult">The jobResult from Nova GET jobs call in decending order by jobId</param>
        /// <param name="pageSize"> pageSize value that indicates number of items that get displayed per page</param>
        /// <param name="expectedOpenAPIJobs"> (optional) jobs that are expected to be found in decending order by jobId, if this is null, it verifies that jobResult.JobInfos is empty</param>
        public static void GetJobsValidation(JobResult jobResult,
            int pageSize,
            List<IOpenAPIJob> expectedOpenAPIJobs = null
            )
        {
            ThrowIf.ArgumentNull(jobResult, nameof(jobResult));

            expectedOpenAPIJobs = expectedOpenAPIJobs ?? new List<IOpenAPIJob>();

            var jobInfoList = jobResult.JobInfos.ToList<IJobInfo>();

            if (expectedOpenAPIJobs.Any())
            {
                // Job Contents comparison and validation
                var compareCount = Math.Min(expectedOpenAPIJobs.Count, pageSize);
                var jobsToBeFoundToCompare = expectedOpenAPIJobs.Take(compareCount).ToList();

                for (int i = 0; i < compareCount; i++)
                {
                    Assert.AreEqual(jobsToBeFoundToCompare[i].JobId, jobInfoList[i].JobId,
                        "The jobId {0} was expected but jobId {1} is returned from GET job or jobs call.",
                        jobsToBeFoundToCompare[i].JobId, jobInfoList[i].JobId);

                    Assert.AreEqual(jobsToBeFoundToCompare[i].ProjectId, jobInfoList[i].ProjectId,
                        "The projectId {0} was expected but projectId {1} is returned from GET job or jobs call.",
                        jobsToBeFoundToCompare[i].ProjectId, jobInfoList[i].ProjectId);

                    Assert.IsTrue(jobsToBeFoundToCompare[i].ProjectName.Contains(jobInfoList[i].Project),
                        "The projectName {0} was expected to contain project value {1} from GET job or jobs call.",
                        jobsToBeFoundToCompare[i].ProjectName, jobInfoList[i].Project);

                    Assert.AreEqual(jobsToBeFoundToCompare[i].JobType, jobInfoList[i].JobType,
                        "The jobType {0} was expected but jobType {1} is returned from GET job or jobs call.",
                        jobsToBeFoundToCompare[i].JobType, jobInfoList[i].JobType);

                    Assert.AreEqual(jobsToBeFoundToCompare[i].SubmittedDateTime.ToStringInvariant(),
                        jobInfoList[i].SubmittedDateTime.ToStringInvariant(), "The SubmittedDateTime {0} was expected but SubmittedDateTime {1} is returned from GET job or jobs call.",
                        jobsToBeFoundToCompare[i].SubmittedDateTime.ToStringInvariant(), jobInfoList[i].SubmittedDateTime.ToStringInvariant());
                }
            }
            else
            {
                Assert.AreEqual(0, jobInfoList.Count(),
                    "The jobInfos from jobResult should be empty when expected return result is empty but the response from the Nova GET job or jobs call returns {0} results",
                    jobInfoList.Count());

                Assert.AreEqual(0, jobResult.TotalJobCount, "The totalJobCount should be 0 when expected return result is empty but the response from the Nova GET job or jobs call returns {0}", jobResult.TotalJobCount);
            }

            // Validation: Verify that jobResult uses pageSize values passed as optional parameters
            Assert.That(jobInfoList.Count() <= pageSize,
                "The expected pagesize value is {0} but {1} was found from the returned searchResult.",
                pageSize, jobInfoList.Count());
        }

        /// <summary>
        /// Asserts that returned jobResult from the Nova GET job or jobs call match with jobs that are being retrieved.
        /// </summary>
        /// <param name="jobResult">The jobResult from Nova GET jobs call in decending order by jobId</param>
        /// <param name="pageSize"> pageSize value that indicates number of items that get displayed per page</param>
        /// <param name="expectedAddJobResults"> (optional) jobs that are expected to be found in decending order by jobId, if this is null, it verifies that jobResult.JobInfos is empty</param>
        public static void GetJobsValidation(JobResult jobResult,
            int pageSize,
            List<AddJobResult> expectedAddJobResults = null
            )
        {
            ThrowIf.ArgumentNull(jobResult, nameof(jobResult));

            expectedAddJobResults = expectedAddJobResults ?? new List<AddJobResult>();

            var jobInfoList = jobResult.JobInfos.ToList<IJobInfo>();

            if (expectedAddJobResults.Any())
            {
                // Job Contents comparison and validation
                var compareCount = Math.Min(expectedAddJobResults.Count, pageSize);
                var jobsToBeFoundToCompare = expectedAddJobResults.Take(compareCount).ToList();

                for (int i = 0; i < compareCount; i++)
                {
                    Assert.AreEqual(jobsToBeFoundToCompare[i].JobId, jobInfoList[i].JobId,
                        "The jobId {0} was expected but jobId {1} is returned from GET job or jobs call.",
                        jobsToBeFoundToCompare[i].JobId, jobInfoList[i].JobId);
                }
            }
            else
            {
                Assert.AreEqual(0, jobInfoList.Count(),
                    "The jobInfos from jobResult should be empty when expected return result is empty but the response from the Nova GET job or jobs call returns {0} results",
                    jobInfoList.Count());

                Assert.AreEqual(0, jobResult.TotalJobCount, "The totalJobCount should be 0 when expected return result is empty but the response from the Nova GET job or jobs call returns {0}", jobResult.TotalJobCount);
            }

            // Validation: Verify that jobResult uses pageSize values passed as optional parameters
            Assert.That(jobInfoList.Count() <= pageSize,
                "The expected pagesize value is {0} but {1} was found from the returned searchResult.",
                pageSize, jobInfoList.Count());
        }

        /// <summary>
        /// Asserts that returned jobInfo from the Nova GET Job call match with job that are being retrieved.
        /// </summary>
        /// <param name="jobInfo">The jobInfo from Nova GET job call</param>
        /// <param name="expectedOpenAPIJob"> (optional) job that are expected to be found, if this is null, job content validation step gets skipped.</param>
        public static void GetJobValidation(IJobInfo jobInfo, IOpenAPIJob expectedOpenAPIJob)
        {
            // creating the jobResult with the empty TotalJobCount
            var jobResult = new JobResult();
            var jobInfoList = new List<IJobInfo>() { jobInfo }.ConvertAll(o => (JobInfo)o);

            jobResult.JobInfos = jobInfoList;
            jobResult.TotalJobCount = 0;

            GetJobsValidation(jobResult: jobResult, pageSize: 1, expectedOpenAPIJobs: new List<IOpenAPIJob>() { expectedOpenAPIJob });
        }

        #endregion Custom Asserts

        #endregion Job Management

        #region User Management

        public const uint MinPasswordLength = 8;
        public const uint MaxPasswordLength = 128;
        public const InstanceAdminPrivileges AllPrivileges =
                            InstanceAdminPrivileges.AccessAllProjectData |
                            InstanceAdminPrivileges.ManageInstanceSettings |
                            InstanceAdminPrivileges.ManageAdminRoles |
                            InstanceAdminPrivileges.DeleteProjects |
                            InstanceAdminPrivileges.AssignAdminRoles |
                            InstanceAdminPrivileges.ManageStandardPropertiesAndArtifactTypes |
                            InstanceAdminPrivileges.CanManageAllRunningJobs |
                            InstanceAdminPrivileges.CanReportOnAllProjects;

        /// <summary>
        /// A class to represent a row in the PasswordRecoveryTokens database table.
        /// </summary>
        public class PasswordRecoveryToken
        {
            public string Login { get; set; }
            public DateTime CreationTime { get; set; }
            public string RecoveryToken { get; set; }
        }

        /// <summary>
        /// Verifies that the specified user is disabled.
        /// </summary>
        /// <param name="helper">A TestHelper object.</param>
        /// <param name="adminUser">The user to authenticate with.</param>
        /// <param name="user">The user who should be disabled.</param>
        public static void AssertUserIsDisabled(TestHelper helper, IUser adminUser, IUser user)
        {
            ThrowIf.ArgumentNull(helper, nameof(helper));
            ThrowIf.ArgumentNull(user, nameof(user));

            var returnedUser = helper.OpenApi.GetUser(adminUser, user.Id);

            Assert.NotNull(returnedUser, "GetUser returned null for User ID: {0}!", user.Id);
            Assert.IsFalse(returnedUser.Enabled.Value, "The user is enabled, but should be disabled!");
        }

        /// <summary>
        /// Verifies that the specified user ID is deleted or doesn't exist.
        /// </summary>
        /// <param name="helper">A TestHelper object.</param>
        /// <param name="adminUser">An admin user with an OpenAPI token to make the OpenAPI call.</param>
        /// <param name="userId">The ID of the user whose existence is being checked.</param>
        public static void AssertUserNotFound(TestHelper helper, IUser adminUser, int userId)
        {
            var ex = Assert.Throws<Http404NotFoundException>(() => helper.OpenApi.GetUser(adminUser, userId),
                "GetUser should return 404 Not Found for a deleted or non-existing user.");

            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, "The requested user is not found.");
        }

        /// <summary>
        /// Generates a valid random password of the specified length.  NOTE: Length must be between 8 and 128.
        /// </summary>
        /// <param name="length">The length of the password to generate.</param>
        /// <returns>A new valid random password.</returns>
        public static string GenerateValidPassword(uint length = MinPasswordLength)
        {
            if ((length < MinPasswordLength) || (length > MaxPasswordLength))
            {
                throw new ArgumentOutOfRangeException(nameof(length),
                    I18NHelper.FormatInvariant("The length must be between {0} and {1}!",
                    MinPasswordLength, MaxPasswordLength));
            }

            return RandomGenerator.RandomUpperCase(length - 2) + "1$";
        }

        /// <summary>
        /// Gets the latest PasswordRecoveryToken from the AdminStore database for the specified username.
        /// </summary>
        /// <param name="username">The username whose recovery token you want to get.</param>
        /// <returns>The latest PasswordRecoveryToken for the specified user, or null if no token was found for that user.</returns>
        public static PasswordRecoveryToken GetRecoveryTokenFromDatabase(string username)
        {
            string query = I18NHelper.FormatInvariant(
                "SELECT * FROM [dbo].[PasswordRecoveryTokens] WHERE [Login] = '{0}' ORDER BY [CreationTime] DESC", username);

            var columnNames = new List<string> { "Login", "CreationTime", "RecoveryToken" };

            try
            {
                var results = DatabaseHelper.ExecuteMultipleValueSqlQuery(query, columnNames, "AdminStore");
                string createTime = results["CreationTime"];

                return new AdminStoreHelper.PasswordRecoveryToken
                {
                    Login = results["Login"],
                    CreationTime = DateTime.Parse(createTime, CultureInfo.InvariantCulture),
                    RecoveryToken = results["RecoveryToken"]
                };
            }
            catch (SqlQueryFailedException)
            {
                Logger.WriteDebug("No PasswordRecoveryToken was found for user: {0}", username);
            }

            return null;
        }

        /// <summary>
        /// Generates a random Instance User for user management
        /// </summary>
        /// <param name="loginName">(optional) The login name of the user. Randomly generated if not specified.</param>
        /// <param name="password">(optional) The user's password. Randomly generated if not specified.</param>
        /// <param name="source">(optional) The source of the user. Defaults to Database if not specified.</param>
        /// <param name="displayname">(optional) The user's display name. Randomly generated if not specified.</param>
        /// <param name="licenseLevel">(optinal) The user's license level. Defaults to Author if not specified.</param>
        /// <param name="instanceAdminRole">(optional) The user's instance admin role. Defaults to DefaultInstanceAdministrator.</param>
        /// <param name="adminPrivileges">(optional) The user's instance admin privileges.  Defaults to None.</param>
        /// <param name="imageId">(optional) The user's image id. Defaults to null.</param>
        /// <returns>An InstanceUser object.</returns>
        public static InstanceUser GenerateRandomInstanceUser(
            string loginName = null,
            string password = null,
            UserSource source = UserSource.Database,
            string displayname = null,
            LicenseLevel licenseLevel = LicenseLevel.Viewer,
            InstanceAdminRole? instanceAdminRole = null,
            InstanceAdminPrivileges adminPrivileges = InstanceAdminPrivileges.None,
            int? imageId = null)
        {
            string login = loginName ?? RandomGenerator.RandomAlphaNumeric(MinPasswordLength);
            string firstName = RandomGenerator.RandomAlphaNumeric(10);
            string lastName = RandomGenerator.RandomAlphaNumeric(10);
            string email = I18NHelper.FormatInvariant("{0}@{1}.com", login, RandomGenerator.RandomAlphaNumeric(10));
                                                        
            var instanceUser = new InstanceUser
            (
                login,
                firstName,
                lastName,
                displayname ?? I18NHelper.FormatInvariant("{0} {1}", firstName, lastName),
                email,
                source,
                eulaAccepted: false,
                license: licenseLevel,
                isSso: false,
                allowFallback: false,
                instanceAdminRole: instanceAdminRole,
                instanceAdminPrivileges: adminPrivileges,
                guest: false,
                currentVersion: -1,
                enabled: true,
                title: RandomGenerator.RandomAlphaNumeric(10),
                department: RandomGenerator.RandomAlphaNumeric(10),
                expirePassword: false,
                imageId: imageId,
                groupMembership: null,
                password: password ?? GenerateValidPassword()
            );

            return instanceUser;
        }

        /// <summary>
        /// Asserts that 2 Instance Users are equal
        /// </summary>
        /// <param name="expectedUser">The first instance user being compared.</param>
        /// <param name="actualUser">The second instanace user being compared.</param>
        /// <param name="skipId">(optional) True to compare Ids, false otherwise (Default: true)</param>
        public static void AssertAreEqual(InstanceUser expectedUser, InstanceUser actualUser, bool skipId = true)
        {
            ThrowIf.ArgumentNull(expectedUser, nameof(expectedUser));
            ThrowIf.ArgumentNull(actualUser, nameof(actualUser));

            if (!skipId)
            {
                Assert.AreEqual(expectedUser.Id, actualUser.Id, "Id is different. Expected: {0} Actual: {1}", expectedUser.Id, actualUser.Id);
            }

            Assert.AreEqual(expectedUser.Login, actualUser.Login, "Login is different. Expected: {0} Actual: {1}", expectedUser.Login, actualUser.Login);
            Assert.AreEqual(expectedUser.FirstName, actualUser.FirstName, "FirstName is different. Expected: {0} Actual: {1}", expectedUser.FirstName, actualUser.FirstName);
            Assert.AreEqual(expectedUser.LastName, actualUser.LastName, "LastName is different. Expected: {0} Actual: {1}", expectedUser.LastName, actualUser.LastName);
            Assert.AreEqual(expectedUser.DisplayName, actualUser.DisplayName, "DisplayName is different. Expected: {0} Actual: {1}", expectedUser.DisplayName, actualUser.DisplayName);
            Assert.AreEqual(expectedUser.Email, actualUser.Email, "Email is different. Expected: {0} Actual: {1}", expectedUser.Email, actualUser.Email);
            Assert.AreEqual(expectedUser.Source, actualUser.Source, "Source is different. Expected: {0} Actual: {1}", expectedUser.Source, actualUser.Source);
            Assert.AreEqual(expectedUser.EULAAccepted, actualUser.EULAAccepted, "EULAAccepted is different. Expected: {0} Actual: {1}", expectedUser.EULAAccepted, actualUser.EULAAccepted);
            Assert.AreEqual(expectedUser.LicenseType, actualUser.LicenseType, "LicenseType is different. Expected: {0} Actual: {1}", expectedUser.LicenseType, actualUser.LicenseType);
            Assert.AreEqual(expectedUser.IsSso, actualUser.IsSso, "IsSso is different. Expected: {0} Actual: {1}", expectedUser.IsSso, actualUser.IsSso);
            Assert.AreEqual(expectedUser.AllowFallback, actualUser.AllowFallback, "AllowFallback is different. Expected: {0} Actual: {1}", expectedUser.AllowFallback, actualUser.AllowFallback);
            Assert.AreEqual(expectedUser.InstanceAdminRoleId, actualUser.InstanceAdminRoleId, "InstanceAdminRoleId is different. Expected: {0} Actual: {1}", expectedUser.InstanceAdminRoleId, actualUser.InstanceAdminRoleId);
            Assert.AreEqual(expectedUser.InstanceAdminPrivileges, actualUser.InstanceAdminPrivileges, "InstanceAdminPrivileges is different. Expected: {0} Actual: {1}", expectedUser.InstanceAdminPrivileges, actualUser.InstanceAdminPrivileges);
            Assert.AreEqual(expectedUser.Guest, actualUser.Guest, "Guest is different. Expected: {0} Actual: {1}", expectedUser.Guest, actualUser.Guest);
            Assert.AreEqual(expectedUser.Enabled, actualUser.Enabled, "Enabled is different. Expected: {0} Actual: {1}", expectedUser.Enabled, actualUser.Enabled);
            Assert.AreEqual(expectedUser.Title, actualUser.Title, "Title is different. Expected: {0} Actual: {1}", expectedUser.Title, actualUser.Title);
            Assert.AreEqual(expectedUser.Department, actualUser.Department, "Department is different. Expected: {0} Actual: {1}", expectedUser.Department, actualUser.Department);
            Assert.AreEqual(expectedUser.ExpirePassword, actualUser.ExpirePassword, "ExpirePassword is different. Expected: {0} Actual: {1}", expectedUser.ExpirePassword, actualUser.ExpirePassword);
            Assert.AreEqual(expectedUser.ImageId, actualUser.ImageId, "ImageId is different. Expected: {0} Actual: {1}", expectedUser.ImageId, actualUser.ImageId);


            Assert.AreEqual(expectedUser.GroupMembership.Length, actualUser.GroupMembership.Length, "GroupMembership count is different. Expected: {0} Actual: {1}", expectedUser.GroupMembership.Length, actualUser.GroupMembership.Length);

            foreach (var group in expectedUser.GroupMembership)
            {
                Assert.Contains(group, actualUser.GroupMembership, "Expected group {0} is not found in actual group membership.", group);
            }
        }

        #endregion User Management
    }
}
