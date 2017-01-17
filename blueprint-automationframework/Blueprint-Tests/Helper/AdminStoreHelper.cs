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
using Utilities;

namespace Helper
{

    public static class AdminStoreHelper
    {

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
        /// <param name="expectedJobs"> (optional) jobs that are expected to be found in decending order by jobId, if this is null, it verifies that jobResult.JobInfos is empty</param>
        public static void JobResultValidation<T>(JobResult jobResult,
            int pageSize,
            List<T> expectedJobs = null
            ) where T : new ()
        {
            ThrowIf.ArgumentNull(jobResult, nameof(jobResult));

            expectedJobs = expectedJobs ?? new List<T>();

            var jobInfoList = jobResult.JobInfos.ToList<IJobInfo>();

            if (expectedJobs.Any())
            {
                // Job Contents comparison and validation
                var compareCount = Math.Min(expectedJobs.Count, pageSize);
                var jobsToBeFoundToCompare = expectedJobs.Take(compareCount).ToList();

                for (int i = 0; i < compareCount; i++)
                {
                    if (expectedJobs.GetType().Equals(typeof(List<AddJobResult>)))
                    {
                        var jobToBeFoundToCompareForAddJobResult = jobsToBeFoundToCompare[i] as AddJobResult;

                        Assert.AreEqual(jobToBeFoundToCompareForAddJobResult.JobId, jobInfoList[i].JobId,
                        "The jobId {0} was expected but jobId {1} is returned from GET job or jobs call.",
                        jobToBeFoundToCompareForAddJobResult.JobId, jobInfoList[i].JobId);
                    }

                    if (expectedJobs.GetType().Equals(typeof(List<IOpenAPIJob>)))
                    {
                        var jobToBeFoundToCompareForOpenAPIJobType = jobsToBeFoundToCompare[i] as IOpenAPIJob;

                        Assert.AreEqual(jobToBeFoundToCompareForOpenAPIJobType.JobId, jobInfoList[i].JobId,
                        "The jobId {0} was expected but jobId {1} is returned from GET job or jobs call.",
                        jobToBeFoundToCompareForOpenAPIJobType.JobId, jobInfoList[i].JobId);

                        Assert.AreEqual(jobToBeFoundToCompareForOpenAPIJobType.ProjectId, jobInfoList[i].ProjectId,
                        "The projectId {0} was expected but projectId {1} is returned from GET job or jobs call.",
                        jobToBeFoundToCompareForOpenAPIJobType.ProjectId, jobInfoList[i].ProjectId);

                        Assert.IsTrue(jobToBeFoundToCompareForOpenAPIJobType.ProjectName.Contains(jobInfoList[i].Project),
                        "The projectName {0} was expected to contain project value {1} from GET job or jobs call.",
                        jobToBeFoundToCompareForOpenAPIJobType.ProjectName, jobInfoList[i].Project);

                        Assert.AreEqual(jobToBeFoundToCompareForOpenAPIJobType.JobType, jobInfoList[i].JobType,
                        "The jobType {0} was expected but jobType {1} is returned from GET job or jobs call.",
                        jobToBeFoundToCompareForOpenAPIJobType.JobType, jobInfoList[i].JobType);

                        Assert.AreEqual(jobToBeFoundToCompareForOpenAPIJobType.SubmittedDateTime.ToStringInvariant(),
                        jobInfoList[i].SubmittedDateTime.ToStringInvariant(), "The SubmittedDateTime {0} was expected but SubmittedDateTime {1} is returned from GET job or jobs call.",
                        jobToBeFoundToCompareForOpenAPIJobType.SubmittedDateTime.ToStringInvariant(), jobInfoList[i].SubmittedDateTime.ToStringInvariant());
                    }
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
        /// <param name="expectedJob"> (optional) job that are expected to be found, if this is null, job content validation step gets skipped.</param>
        public static void JobResultValidation(JobInfo jobInfo, OpenAPIJob expectedJob = null)
        {
            // creating the jobResult with the empty TotalJobCount
            JobResult jobResult = new JobResult();
            var jobInfoList = new List<IJobInfo>() { jobInfo }.ConvertAll(o => (JobInfo)o);
            jobResult.JobInfos = jobInfoList;
            jobResult.TotalJobCount = 0;

            JobResultValidation(jobResult: jobResult, pageSize: 1, expectedJobs: new List<OpenAPIJob>() { expectedJob });
        }
    }

    #endregion Custom Asserts

}
