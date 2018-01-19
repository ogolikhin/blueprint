using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories.ArtifactListSetting
{
    public interface IArtifactListSettingsRepository
    {
        Task<int> CreateArtifactListSettingsAsync(int collectionId, int userId, string settings);
        Task<int> UpdateArtifactListSettingsAsync(int collectionId, int userId, string settings);
    }
}
