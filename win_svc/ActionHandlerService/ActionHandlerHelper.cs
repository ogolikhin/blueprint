namespace ActionHandlerService
{
    public interface IActionHelper
    {
        bool HandleAction(TenantInfo tenant);
    }

    //We should be creating specific action handlers for different  message handlers. 
    //These should be implemented when the actions are impletemented
    public class GenerateTestsActionHelper : IActionHelper
    {
        public bool HandleAction(TenantInfo tenant)
        {
            return true;
        }
    }

    public class GenerateUserStoriesActionHelper : IActionHelper
    {
        public bool HandleAction(TenantInfo tenant)
        {
            return true;
        }
    }

    public class GenerateDescendantsActionHelper : IActionHelper
    {
        public bool HandleAction(TenantInfo tenant)
        {
            return true;
        }
    }

    public class NotificationActionHelper : IActionHelper
    {
        public bool HandleAction(TenantInfo tenant)
        {
            return true;
        }
    }
}
