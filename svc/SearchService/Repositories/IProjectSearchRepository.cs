﻿using ArtifactStore.Models;
using SearchService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchService.Repositories
{
    public interface IProjectSearchRepository
    {
        Task<IEnumerable<ProjectSearchResult>> GetProjectsByName(int userId, string searchText, int resultCount);

        Task<IEnumerable<ItemSearchResult>> FindItemByName(int userId, string searchText, int[] projectIds, int[] itemTypes, int resultCount);
    }
}
