using ServiceLibrary.Models;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public interface IInstanceSettingsRepository
    {
        Task<EmailSettings> GetEmailSettings();
    }
}
