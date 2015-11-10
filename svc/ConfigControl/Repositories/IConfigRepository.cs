using System.Collections.Generic;
using System.Threading.Tasks;
using ConfigControl.Models;

namespace ConfigControl.Repositories
{
    public interface IConfigRepository
    {
        Task<IEnumerable<ConfigSetting>> GetSettings(bool allowRestricted);
    }
}
