using System.Collections.Generic;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DEmailNotificationAction : DBaseAction
    {
        public override ActionTypes ActionType => ActionTypes.EmailNotification;
        public IEnumerable<string> Emails { get; set; }
        public string PropertyName { get; set; }
        public int? PropertyId { get; set; }
        public string Message { get; set; }
    }
}