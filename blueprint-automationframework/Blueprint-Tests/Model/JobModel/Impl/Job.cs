using Common;
using Model.JobModel.Enums;
using System;
using System.Collections.Generic;
using System.Net;
using Utilities;
using Utilities.Facades;

namespace Model.JobModel.Impl
{
    public class OpenAPIJob : IOpenAPIJob
    {
        #region properties

        public DateTime SubmittedDateTime { get; set; }
        public DateTime? JobStartDateTime { get; set; }
        public int JobId { get; set; }
        public JobStatus Status { get; set; }
        public JobType JobType { get; set; }
        public string ProjectName { get; set; }
        public int? UserId { get; set; }
        public string UserDisplayName { get; set; }
        public string JobServerName { get; set; }
        public string Progress { get; set; } //decimal
        public string JobOutput { get; set; }
        public bool IsJobMarkedForCancellation { get; set; }
        public int? ProjectId { get; set; }

        #endregion properties

        #region static methods

        public static IJobBase AddAlmChangeSummaryJob (string address,
            IUser user,
            IProject project,
            int baselineArtifactId,
            int targetId,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(project, nameof(project));

            RestApiFacade restApi = new RestApiFacade(address, user?.Token?.OpenApiToken);

            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.Targets_id_.JOBS, project.Id,targetId);

            var almJob = new AlmJob(AlmJobType.ChangeSummary, baselineArtifactId, project);
            var returnedAlmChangeSummaryJob = restApi.SendRequestAndDeserializeObject<OpenAPIJob, AlmJob>(
                path,
                RestRequestMethod.POST,
                almJob,
                expectedStatusCodes: expectedStatusCodes);

            return returnedAlmChangeSummaryJob;
        }

        #endregion static methods
    }

    public class JobInfo : IJobInfo
    {
        #region properties
        public int JobId { get; set; }

        public JobStatus Status { get; set; } //JobStatus

        public JobType JobType { get; set; } //JobType

        public string Project { get; set; }

        public DateTime SubmittedDateTime { get; set; }

        public DateTime? JobStartDateTime { get; set; }

        public DateTime? JobEndDateTime { get; set; }

        public int? UserId { get; set; }

        public string UserDisplayName { get; set; }

        public string Server { get; set; }

        public string Progress { get; set; } //decimal

        public string Output { get; set; }

        public bool StatusChanged { get; set; }

        public bool HasCancelJob { get; set; }

        public int? ProjectId { get; set; }

        #endregion properties


    }
    public class AlmJob
    {
        #region Constructors

        public AlmJob()
        {
            // this is for deserialization
        }

        public AlmJob(AlmJobType almJobType, int baselineOrReviewId, IProject project)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            this.AlmJobType = almJobType.ToString();
            this.JobParameters = new JobParameters()
            {
                Type = "ChangeSummaryParameters",
                IsImageGenerationRequired = false,
                IsFirstPush = true,
                BaselineOrReviewId = baselineOrReviewId,
                AlmRootPathId = project.Id,
                AlmRootPath = project.Name
            };

        }

        #endregion Constructors

        #region properties

        public string AlmJobType { get; set; }

        public JobParameters JobParameters { get; set; }
        
        #endregion properties

    }

    public class JobParameters
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public string Type { get; set; }
        public bool IsImageGenerationRequired { get; set; }
        public bool IsFirstPush { get; set; }
        public int BaselineOrReviewId { get; set; }
        public int AlmRootPathId { get; set; }
        public string AlmRootPath { get; set; }
    }

}
