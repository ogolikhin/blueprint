using BluePrintSys.Messaging.CrossCutting;

namespace ActionHandlerService.Helpers
{
    public class ExtendedConfigHelper
    {
        public const string ServiceNameKey = "Service.Name";
        public const string ServiceNameDefault = "BlueprintWorkflowService";
        public string ServiceName => AppSettingsHelper.GetConfigStringValue(ServiceNameKey, ServiceNameDefault);
    }
}
