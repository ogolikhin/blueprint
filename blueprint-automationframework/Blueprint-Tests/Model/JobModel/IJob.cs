using Model.JobModel.Enums;
using System;

namespace Model.JobModel
{
    public interface IJobBase
    {
        #region properties

        int JobId { get; set; }
        JobStatus Status { get; set; }
        JobType JobType { get; set; }
        DateTime SubmittedDateTime { get; set; }
        DateTime? JobStartDateTime { get; set; }
        int? UserId { get; set; }
        string UserDisplayName { get; set; }
        string Progress { get; set; }
        int? ProjectId { get; set; }

        #endregion properties
    }

    public interface IOpenAPIJob : IJobBase
    {

        #region properties

        string ProjectName { get; set; }
        string JobServerName { get; set; }
        string JobOutput { get; set; }
        bool IsJobMarkedForCancellation { get; set; }

        #endregion properties

    }
    public interface IJobInfo : IJobBase
    {
        #region properties

        string Project { get; set; }
        DateTime? JobEndDateTime { get; set; }
        string Server { get; set; }
        string Output { get; set; }
        bool StatusChanged { get; set; }
        bool HasCancelJob { get; set; }

        #endregion properties

    }
}
