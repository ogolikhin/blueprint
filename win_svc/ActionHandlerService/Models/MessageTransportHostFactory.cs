using ActionHandlerService.Helpers;
using ActionHandlerService.Models.Enums;

namespace ActionHandlerService.Models
{
    public class MessageTransportHostFactory
    {
        public static IMessageTransportHost GetMessageTransportHost()
        {
            if (ConfigHelper.Broker == MessageBroker.SQL)
            {
                return new SqlTransportHost();
            }
            return new RabbitMQTransportHost();
        }
    }
}
