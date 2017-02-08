using Model.JobModel.Enums;
using System;

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
        public string Progress { get; set; }
        public string JobOutput { get; set; }
        public bool IsJobMarkedForCancellation { get; set; }
        public int? ProjectId { get; set; }

        #endregion properties
    }

    public class JobInfo : IJobInfo
    {
        #region properties

        public int JobId { get; set; }
        public JobStatus Status { get; set; }
        public JobType JobType { get; set; }
        public string Project { get; set; }
        public DateTime SubmittedDateTime { get; set; }
        public DateTime? JobStartDateTime { get; set; }
        public DateTime? JobEndDateTime { get; set; }
        public int? UserId { get; set; }
        public string UserDisplayName { get; set; }
        public string Server { get; set; }
        public decimal Progress { get; set; }
        public string Output { get; set; }
        public bool StatusChanged { get; set; }
        public bool HasCancelJob { get; set; }
        public int? ProjectId { get; set; }

        #endregion properties


    }
    public class AlmJob
    {
        #region Constants

        private const string ALMROOTPATH = "Requirements/BP Airways";
        private const int ALMROOTPATHID = 12;

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Constructor needed to deserialize it as generic type.
        /// </summary>
        public AlmJob()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="almJobType">The type of ALM job.</param>
        /// <param name="baselineOrReviewId">The baseline or review artifact ID.</param>
        public AlmJob(AlmJobType almJobType, int baselineOrReviewId)
        {
            this.AlmJobType = almJobType.ToString();
            this.JobParameters = new JobParameters
            {
                Type = "ChangeSummaryParameters",
                IsImageGenerationRequired = false,
                IsFirstPush = true,
                BaselineOrReviewId = baselineOrReviewId,
                AlmRootPathId = ALMROOTPATHID,
                AlmRootPath = ALMROOTPATH
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
