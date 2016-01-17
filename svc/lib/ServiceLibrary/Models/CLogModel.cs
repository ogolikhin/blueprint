/// *************************************************************************************
/// ***** Any changes to this file need to be replicated in the                     *****
/// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
/// *************************************************************************************
using System;

namespace ServiceLibrary.Models
{
    public class CLogModel : ServiceLogModel
    {
        public string TimeZoneOffset { get; set; }
        public string ActionName { get; set; }
        public double Duration { get; set; }
    }
}
