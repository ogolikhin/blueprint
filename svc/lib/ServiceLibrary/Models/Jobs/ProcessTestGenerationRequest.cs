using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace ServiceLibrary.Models.Jobs
{
    [Serializable]
    public class GenerateProcessTestsJobParameters
    {
        [JsonProperty]
        public int ProjectId { get; set; }
        
        [XmlIgnore]
        [JsonProperty]
        public string ProjectName { get; set; }

        private List<GenerateProcessTestInfo> _processes = new List<GenerateProcessTestInfo>();

        [JsonProperty]
        public List<GenerateProcessTestInfo> Processes => _processes;
    }

    [Serializable]
    public class GenerateProcessTestInfo
    {
        [JsonProperty]
        public int ProcessId { get; set; }
    }
}
