using System;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace AdminStore.Helpers
{
    public class PrivilegesManager
    {
        private readonly IPrivilegesRepository _privilegeRepository;

        public PrivilegesManager(IPrivilegesRepository privilegeRepository)
        {
            if (privilegeRepository == null)
            {
                throw new ArgumentNullException(nameof(privilegeRepository));
            }

            _privilegeRepository = privilegeRepository;
        }


        public async Task Demand(int userId, InstanceAdminPrivileges privileges)
        {
            var currentPrivileges = await _privilegeRepository.GetInstanceAdminPrivilegesAsync(userId);
            if (!currentPrivileges.HasFlag(privileges))
            {
                throw new AuthorizationException(ErrorMessages.UserDoesNotHavePermissions, ErrorCodes.Forbidden);
            }
        }

        public async Task DemandAny(int userId, int projectId, InstanceAdminPrivileges instancePrivileges, ProjectAdminPrivileges projectPrivileges)
        {
            var instancePermissions = await _privilegeRepository.GetInstanceAdminPrivilegesAsync(userId);
            if (!instancePermissions.HasFlag(instancePrivileges))
            {
                var projectPermissions = await _privilegeRepository.GetProjectAdminPermissionsAsync(userId, projectId);
                if (!projectPermissions.HasFlag(projectPrivileges))
                {
                    throw new AuthorizationException(ErrorMessages.UserDoesNotHavePermissions, ErrorCodes.Forbidden);
                }
            }          
        }
    }
}