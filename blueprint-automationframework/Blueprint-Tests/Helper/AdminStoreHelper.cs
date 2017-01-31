using Common;
using Model;
using Model.ArtifactModel;
using Model.Impl;
using Model.JobModel;
using Model.JobModel.Impl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Utilities;

namespace Helper
{

    public static class AdminStoreHelper
    {

        #region Project Management

        #endregion Project Management

        #region Custom Asserts

        /// <summary>
        /// Validate InstanceProject returned from Get ProjectById
        /// </summary>
        /// <param name="helper">A TestHelper instance.</param>
        /// <param name="user">A user used to retrieve InstanceProject</param>
        /// <param name="instanceProject">instanceProject that is returned from Get ProjectById</param>
        /// <param name="project">the project used to validate returned instanceProject</param>
        public static void GetProjectByIdValidation(TestHelper helper, IUser user, IProject project, InstanceProject instanceProject)
        {
            ThrowIf.ArgumentNull(helper, nameof(helper));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(instanceProject, nameof(InstanceProject));

            Assert.AreEqual(project.Id, instanceProject.Id, "Project Id {0} was expected but {1} was returned.",
                project.Id, instanceProject.Id);

            Assert.AreEqual(project.Name, instanceProject.Name, "Project Name '{0}' was expected but '{1}' was returned.", project.Name, instanceProject.Name);

            Assert.IsNotNull(instanceProject.ParentFolderId, "{0} should not be null!", nameof(InstanceProject.ParentFolderId));

            Assert.AreEqual(InstanceItemTypeEnum.Project, instanceProject.Type, "Type '{0}' was expected but '{1}' was returned.", InstanceItemTypeEnum.Project, instanceProject.Type);

            // assign admin-equivalent RolePermission to the project if the user is default instance admin
            // TODO: This should later move to user creation step
            if (user.InstanceAdminRole == InstanceAdminRole.DefaultInstanceAdministrator)
            {
                helper.AssignProjectRolePermissionsToUser(
                user,
                RolePermissions.Read |
                RolePermissions.Edit |
                RolePermissions.Delete |
                RolePermissions.Trace |
                RolePermissions.Comment |
                RolePermissions.StealLock |
                RolePermissions.CanReport |
                RolePermissions.Share |
                RolePermissions.Reuse |
                RolePermissions.ExcelUpdate |
                RolePermissions.DeleteAnyComment |
                RolePermissions.CreateRapidReview,
                project);
            }

            var permissionForProject = helper.ProjectRoles.Find(p => p.ProjectId.Equals(project.Id))?.Permissions;

            Assert.AreEqual(permissionForProject, instanceProject.Permissions,
                "Permission {0} was expected but {1} is returned.", permissionForProject, instanceProject.Permissions);

            if(!string.IsNullOrEmpty(project.Description))
            {
                var plainInstanceProjectDescription = WebUtility.HtmlDecode(
                    Regex.Replace(instanceProject.Description, "<(.|\n)*?>", "")
                    );
                StringAssert.Contains(project.Description, instanceProject.Description, "Project Description '{0}' was expected but '{1}' was returned.", project.Description, plainInstanceProjectDescription);
            }
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

        #endregion Job Management

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
    }

    #endregion Custom Asserts

}
