using System;
using AdminStore.Models;
using AdminStore.Repositories;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Data;
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

            if (!TryGetProjectStatusIfProjectExist(project, out projectStatus))
            {
                throw new ResourceNotFoundException(
                    I18NHelper.FormatInvariant(ErrorMessages.ProjectWasDeletedByAnotherUser, project.Id,
                        project.Name), ErrorCodes.ResourceNotFound);
            }

            if (projectStatus == ProjectStatus.Live)
            {
                Func<IDbTransaction, Task> action = async transaction =>
                {
                    await _instanceRepository.RemoveProject(userId, projectId);
                    await _instanceRepository.DeactivateWorkflowIfLastProjectDeleted(projectId);
                };

                await _instanceRepository.RunInTransactionAsync(action);
            }
            else
            {
                await _instanceRepository.PurgeProject(projectId, project);
            }
        }

        #region private methods

        /// <summary>
        /// Maps the project status string to enum.
        /// </summary>
        private static ProjectStatus GetProjectStatus(string status)
        {
            // Project status is used to identify different status of the import process a project can be in
            switch (status)
            {
                case "I":
                    return ProjectStatus.Importing;
                case "F":
                    return ProjectStatus.ImportFailed;
                case "C":
                    return ProjectStatus.CancelingImport;
                case null:
                    return ProjectStatus.Live;
                default:
                    throw new Exception(I18NHelper.FormatInvariant(ErrorMessages.UnhandledStatusOfProject, status));
            }
        }

        /// <summary>
        ///  This method takes the projectId and checks if the project is still exist in the database and not marked as deleted
        /// </summary>
        /// <param name="project">Project</param>
        /// <param name="projectStatus">If the project exists it returns ProjectStatus as output If the Project does not exists projectstatus = null</param>
        /// <returns>Returns true if project exists in the database and not marked as deleted for that specific revision</returns>
        public bool TryGetProjectStatusIfProjectExist(InstanceItem project, out ProjectStatus? projectStatus)
        {
            if (project == null)
            {
                projectStatus = null;
                return false;
            }
            if (project.ParentFolderId == null)
            {
                projectStatus = null;
                return false;
            }

            projectStatus = GetProjectStatus(project.ProjectStatus);
            return true;
        }

        #endregion


    }
}
