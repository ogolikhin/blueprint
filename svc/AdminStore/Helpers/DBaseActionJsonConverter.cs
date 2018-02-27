using System;
using AdminStore.Models.DiagramWorkflow;
using Newtonsoft.Json.Linq;

namespace AdminStore.Helpers
{
    public class DBaseActionJsonConverter : JsonTypeConverter<DBaseAction>
    {
        protected override DBaseAction Create(Type objectType, JObject jObject)
        {
            switch (jObject.GetValue("ActionType", StringComparison.OrdinalIgnoreCase)?.Value<string>())
            {
                case "0":
                    return new DEmailNotificationAction();
                case "1":
                    return new DPropertyChangeAction();
                case "2":
                    return new DGenerateAction();
                case "3":
                    return new DWebhookAction();
                default:
                    return null;
            }
        }
    }
}