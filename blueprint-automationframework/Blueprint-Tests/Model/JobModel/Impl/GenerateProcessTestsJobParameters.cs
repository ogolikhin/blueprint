using System.Collections.Generic;

namespace Model.JobModel.Impl
{
    public class GenerateProcessTestsJobParameters
    {
        #region properties

        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public List<GenerateProcessTestInfo> Processes { get; set; }

        #endregion properties

        #region constructors

        public GenerateProcessTestsJobParameters()
        {
            // for deserialization
        }

        public GenerateProcessTestsJobParameters(int projectId, string projectName, List<GenerateProcessTestInfo> processes)
        {
            ProjectId = projectId;
            ProjectName = projectName;
            Processes = processes;
        }

        #endregion constructors
    }

    public class GenerateProcessTestInfo
    {
        #region properties

        public int ProcessId { get; set; }

        #endregion properties

        #region constructors

        public GenerateProcessTestInfo()
        {
            //for deserialization
        }

        public GenerateProcessTestInfo(int processId)
        {
            ProcessId = processId;
        }

        #endregion constructors
    }
}