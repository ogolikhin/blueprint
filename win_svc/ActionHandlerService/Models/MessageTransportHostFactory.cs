using ActionHandlerService.Helpers;
using ActionHandlerService.Models.Enums;

namespace ActionHandlerService.Models
{
    public class MessageTransportHostFactory
    {
        private IConfigHelper ConfigHelper { get; }

        public MessageTransportHostFactory(IConfigHelper configHelper = null)
        {
            ConfigHelper = configHelper ?? new ConfigHelper();
        }

        public IMessageTransportHost GetMessageTransportHost()
        {
            if (ConfigHelper.MessageBroker == MessageBroker.SQL)
            {
                return new SqlTransportHost();
            }
            return new RabbitMqTransportHost(ConfigHelper);
        }
    }
}
