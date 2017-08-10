using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLibrary.Services
{
    public interface IFeaturesService
    {
        Task<IDictionary<string, bool>> GetFeaturesAsync();
    }
}