using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models.DTO;
using AdminStore.Repositories;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace AdminStore.Services.Instance
{
    public class InstanceService : IInstanceService
    {
        private readonly IInstanceRepository _instanceRepository;

        public InstanceService() : this(new SqlInstanceRepository(new SqlConnectionWrapper(ServiceConstants.RaptorMain)))
        {

        }
        public InstanceService(IInstanceRepository instanceRepository)
        {
            _instanceRepository = instanceRepository;
        }

        public async Task<IEnumerable<FolderDto>> GetFoldersByName(string name)
        {
            return await _instanceRepository.GetFoldersByName(name);
        }
    }
}