using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface IInstanceRolesRepository
    {
        Task<IEnumerable<AdminRole>> GetInstanceRolesAsync();
    }
}
