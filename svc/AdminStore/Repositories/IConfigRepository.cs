using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface IConfigRepository
    {
        Task<IEnumerable<ApplicationLabel>> GetLabels(string locale);
    }
}
