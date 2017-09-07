using BluePrintSys.Messaging.CrossCutting;

namespace BlueprintSys.RC.Services.Helpers
{
    public class ExtendedConfigHelper
    {
        public const string ServiceNameKey = "Service.Name";
        public const string ServiceNameDefault = "BlueprintWorkflowService";
        public string ServiceName => AppSettingsHelper.GetConfigStringValue(ServiceNameKey, ServiceNameDefault);
    }
}
