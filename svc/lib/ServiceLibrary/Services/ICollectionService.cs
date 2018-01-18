using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Services
{
    public interface ICollectionsService
    {
        Task<int> SaveArtifactColumnsSettings(int itemId, int userId, string settings);
    }
}
