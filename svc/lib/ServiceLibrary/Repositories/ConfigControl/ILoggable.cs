using System.Security.Cryptography.X509Certificates;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    public interface ILoggable
    {
        IServiceLogRepository Log { get; }

        string LogSource { get; }
    }
}