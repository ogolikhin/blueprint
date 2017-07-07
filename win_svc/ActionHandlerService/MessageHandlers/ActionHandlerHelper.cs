namespace ActionHandlerService.MessageHandlers
{
    public interface IActionHelper
    {
        bool HandleAction(TenantInfo tenant);
    }
}
