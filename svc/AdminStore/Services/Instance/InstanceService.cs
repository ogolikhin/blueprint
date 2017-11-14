using AdminStore.Models;
using AdminStore.Repositories;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models.Enums;
using ServiceLibrary.Exceptions;

namespace AdminStore.Services.Instance
{
    public class InstanceService : IInstanceService
    {
        private readonly IInstanceRepository _instanceRepository;

        public InstanceService() : this(new SqlInstanceRepository(new SqlConnectionWrapper(ServiceConstants.RaptorMain), new SqlHelper()))
        {

        }
        public InstanceService(IInstanceRepository instanceRepository)
        {
            _instanceRepository = instanceRepository;
        }

        public async Task<IEnumerable<InstanceItem>> GetFoldersByName(string name)
        {
            return await _instanceRepository.GetFoldersByName(name);
        }

        public async Task DeleteProject(int userId, int projectId)
        {
            ProjectStatus? projectStatus;

            InstanceItem project = await _instanceRepository.GetInstanceProjectAsync(projectId, userId, fromAdminPortal: true);

            if (!_instanceRepository.TryGetProjectStatusIfProjectExist(project, out projectStatus))
            {
                throw new ResourceNotFoundException(
                    I18NHelper.FormatInvariant(ErrorMessages.ProjectWasDeletedByAnotherUser, project.Id,
                        project.Name), ErrorCodes.ResourceNotFound);
            }

            if (projectStatus == ProjectStatus.Live)
            {
                await _instanceRepository.RemoveProject(userId, projectId);
            }
            else
            {
                await _instanceRepository.PurgeProject(projectId, project);
            }
        }
    }
}
