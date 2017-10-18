using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Reuse;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Models.Workflow.Actions;

namespace ArtifactStore.Executors
{
    public interface IStateChangeExecutorHelper
    {
        Task<ExecutionParameters> BuildTriggerExecutionParameters(
            int userId,
            VersionControlArtifactInfo artifactInfo,
            WorkflowEventTriggers triggers, 
            IDbTransaction transaction = null);
    }
    public class StateChangeExecutorHelper : IStateChangeExecutorHelper
    {
        private IStateChangeExecutorRepositories _stateChangeExecutorRepositories;
        public StateChangeExecutorHelper(IStateChangeExecutorRepositories stateChangeExecutorRepositories)
        {
            _stateChangeExecutorRepositories = 
            _stateChangeExecutorRepositories = stateChangeExecutorRepositories;
        }
        public async Task<ExecutionParameters> BuildTriggerExecutionParameters(
            int userId,
            VersionControlArtifactInfo artifactInfo, 
            WorkflowEventTriggers triggers,
            IDbTransaction transaction = null)
        {
            if (triggers.IsEmpty())
            {
                return null;
            }
            var artifactId = artifactInfo.Id;
            var isArtifactReadOnlyReuse = await _stateChangeExecutorRepositories.ReuseRepository.DoItemsContainReadonlyReuse(new[] { artifactId }, transaction);

            ItemTypeReuseTemplate reuseTemplate = null;
            var artifactId2StandardTypeId = await _stateChangeExecutorRepositories.ReuseRepository.GetStandardTypeIdsForArtifactsIdsAsync(new HashSet<int> { artifactId });
            var instanceItemTypeId = artifactId2StandardTypeId[artifactId].InstanceTypeId;
            if (instanceItemTypeId == null)
            {
                throw new BadRequestException("Artifact is not a standard artifact type");
            }
            if (isArtifactReadOnlyReuse.ContainsKey(artifactId) && isArtifactReadOnlyReuse[artifactId])
            {
                reuseTemplate = await LoadReuseSettings(instanceItemTypeId.Value);
            }
            var customItemTypeToPropertiesMap = await LoadCustomPropertyInformation(new[] { instanceItemTypeId.Value }, triggers, artifactInfo.ProjectId, userId, artifactId);

            var propertyTypes = new List<WorkflowPropertyType>();
            if (customItemTypeToPropertiesMap.ContainsKey(artifactInfo.ItemTypeId))
            {
                propertyTypes = customItemTypeToPropertiesMap[artifactInfo.ItemTypeId];
            }
            var usersAndGroups = await LoadUsersAndGroups(triggers);
            return new ExecutionParameters(
                userId,
                artifactInfo,
                reuseTemplate,
                propertyTypes,
                _stateChangeExecutorRepositories.SaveArtifactRepository,
                transaction,
                new ValidationContext(usersAndGroups.Item1, usersAndGroups.Item2));
        }

        private async Task<ItemTypeReuseTemplate> LoadReuseSettings(int itemTypeId, IDbTransaction transaction = null)
        {
            var reuseSettingsDictionary = await _stateChangeExecutorRepositories.ReuseRepository.GetReuseItemTypeTemplatesAsyc(new[] { itemTypeId }, transaction);

            ItemTypeReuseTemplate reuseTemplateSettings;

            if (reuseSettingsDictionary.Count == 0 || !reuseSettingsDictionary.TryGetValue(itemTypeId, out reuseTemplateSettings))
            {
                return null;
            }

            return reuseTemplateSettings;
        }

        private async Task<Dictionary<int, List<WorkflowPropertyType>>> LoadCustomPropertyInformation(
            IEnumerable<int> instanceItemTypeIds, 
            WorkflowEventTriggers triggers, 
            int projectId,
            int userId,
            int artifactId)
        {
            var propertyChangeActions = triggers.Select(t => t.Action).OfType<PropertyChangeAction>().ToList();
            if (propertyChangeActions.Count == 0)
            {
                return new Dictionary<int, List<WorkflowPropertyType>>();
            }

            var instancePropertyTypeIds = propertyChangeActions.Select(b => b.InstancePropertyTypeId);

            return await _stateChangeExecutorRepositories.WorkflowRepository.GetCustomItemTypeToPropertiesMap(userId, artifactId, projectId, instanceItemTypeIds, instancePropertyTypeIds);
        }

        private async Task<Tuple<IEnumerable<SqlUser>, IEnumerable<SqlGroup>>> LoadUsersAndGroups(WorkflowEventTriggers triggers)
        {
            var userGroups = triggers.Select(a => a.Action).OfType<PropertyChangeUserGroupsAction>().SelectMany(b => b.UserGroups).ToList();
            var userIds = userGroups.Where(u => !u.IsGroup.GetValueOrDefault(false) && u.Id.HasValue).Select(u => u.Id.Value).ToHashSet();
            var groupIds = userGroups.Where(u => u.IsGroup.GetValueOrDefault(false) && u.Id.HasValue).Select(u => u.Id.Value).ToHashSet();

            var users = new List<SqlUser>();
            if (userIds.Any())
            {
                users.AddRange(await _stateChangeExecutorRepositories.UsersRepository.GetExistingUsersByIdsAsync(userIds));
            }
            var groups = new List<SqlGroup>();
            if (groupIds.Any())
            {
                groups.AddRange(
                    await _stateChangeExecutorRepositories.UsersRepository.GetExistingGroupsByIds(groupIds, false));
            }

            return new Tuple<IEnumerable<SqlUser>, IEnumerable<SqlGroup>>(users, groups);
        }
    }
}