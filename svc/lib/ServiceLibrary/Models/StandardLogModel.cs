/// *************************************************************************************
/// ***** Any changes to this file need to be replicated in the                     *****
/// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
/// *************************************************************************************

namespace ServiceLibrary.Models
{
    public class StandardLogModel : ServiceLogModel
    {
        public string TimeZoneOffset { get; set; }
        public string ThreadId { get; set; }
    }
}
