namespace ActionHandlerService
{
    public class ActionHandlerHelper : IActionHandlerHelper
    {
        public bool HandleAction(TenantInfo tenant)
        {
            return true;
        }
    }

    public interface IActionHandlerHelper
    {
        bool HandleAction(TenantInfo tenant);
    }
}
