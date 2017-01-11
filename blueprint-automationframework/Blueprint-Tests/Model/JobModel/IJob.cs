using Model.JobModel.Enums;
using Newtonsoft.Json;
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
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        DateTime? JobStartDateTime { get; set; }
        int? UserId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        string UserDisplayName { get; set; }
        int? ProjectId { get; set; }

        #endregion properties
    }

    public interface IOpenAPIJob : IJobBase
    {

        #region properties

        string ProjectName { get; set; }
        string Progress { get; set; }
        string JobServerName { get; set; }
        string JobOutput { get; set; }
        bool IsJobMarkedForCancellation { get; set; }

        #endregion properties

    }
    public interface IJobInfo : IJobBase
    {
        #region properties

        string Project { get; set; }
        decimal Progress { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        DateTime? JobEndDateTime { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        string Server { get; set; }
        string Output { get; set; }
        bool StatusChanged { get; set; }
        bool HasCancelJob { get; set; }

        #endregion properties

    }
}
