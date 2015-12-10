using log4net;
using log4net.Config;

namespace FileStore.Helpers
{
    public class LogHelper
    {
        public static ILog Log;

        static LogHelper()
        {
            XmlConfigurator.Configure();
            Log = LogManager.GetLogger("FileStoreLoggerAppender");
        }
    }
}