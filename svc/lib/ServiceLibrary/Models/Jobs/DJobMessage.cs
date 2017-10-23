using System;

namespace ServiceLibrary.Models.Jobs
{
    public class DJobMessage
    {
        public int JobMessageId
        {
            get;
            set;
        }
        public JobType Type
        {
            get;
            set;
        }
        public JobStatus? Status
        {
            get;
            set;
        }
        public DateTime? StatusChangedTimestamp
        {
            get;
            set;
        }
        public bool Hidden
        {
            get;
            set;
        }
        public string Parameters
        {
            get;
            set;
        }
        public string ReceiverJobServiceId
        {
            get;
            set;
        }
        public int? UserId
        {
            get;
            set;
        }
        public string UserLogin
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;
            set;
        }
        public int? ProjectId
        {
            get;
            set;
        }
        public string ProjectLabel
        {
            get;
            set;
        }
        public string HostUri
        {
            get;
            set;
        }
        public DateTime? SubmittedTimestamp
        {
            get;
            set;
        }
        public DateTime? StartTimestamp
        {
            get;
            set;
        }
        public DateTime? EndTimestamp
        {
            get;
            set;
        }
        public string StatusMessage
        {
            get;
            set;
        }
        public string StatusDescription
        {
            get;
            set;
        }
        public int Progress
        {
            get;
            set;
        }
        public string Result
        {
            get;
            set;
        }
        public string ExecutorJobServiceId
        {
            get;
            set;
        }
        public DateTime? CurrentTimestamp
        {
            get;
            set;
        }
        public int TotalCount
        {
            get;
            set;
        }
    }
}
