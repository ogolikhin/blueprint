using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ArtifactStore.Repositories.Revisions;
using ArtifactStore.Repositories.VersionControl;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.VersionControl;

namespace ArtifactStore.Services.VersionControl
{
    public class PublishParameters
    {
        public int UserId { get; set; }
        public bool? All { get; set; }
        public IEnumerable<int> ArtifactIds { get; set; }
    }

    public interface IVersionControlService
    {
        Task<ArtifactResultSet> PublishArtifacts(PublishParameters parameters);
    }

    public class VersionControlService : IVersionControlService
    {
        private readonly IVersionControlRepository _versionControlRepository;
        private readonly IPublishRepository _publishRepositoryComposer;
        private readonly IRevisionRepository _revisionRepository;
        private readonly ISqlHelper _sqlHelper;


        public VersionControlService() : this(
            new SqlVersionControlRepository(), 
            new SqlPublishRepositoryComposer(),
            new SqlRevisionRepository(),
            new SqlHelper())
        {
            
        }

        public VersionControlService(
            IVersionControlRepository versionControlRepository, 
            IPublishRepository publishRepository,
            IRevisionRepository revisionRepository,
            ISqlHelper sqlHelper)
        {
            _versionControlRepository = versionControlRepository;
            _publishRepositoryComposer = publishRepository;
            _revisionRepository = revisionRepository;
            _sqlHelper = sqlHelper;
        }

        public async Task<ArtifactResultSet> PublishArtifacts(PublishParameters parameters)
        {
            IList<int> artifactIdsList;
            ICollection<SqlDiscardPublishState> discardPublishStates;

            if (parameters.All.HasValue && parameters.All.Value)
            {
                discardPublishStates = await _versionControlRepository.GetAllDiscardPublish(parameters.UserId);
                artifactIdsList = discardPublishStates.Select(dps => dps.ItemId).ToList();
            }
            else
            {
                artifactIdsList = (parameters.ArtifactIds ?? Enumerable.Empty<int>()).Distinct().ToList();
                if (!artifactIdsList.Any())
                {
                    throw new BadRequestException("The list of artifact Ids is empty.", ErrorCodes.IncorrectInputParameters);
                }
                discardPublishStates = await _versionControlRepository.GetDiscardPublishStates(parameters.UserId, artifactIdsList);
            }
            if (artifactIdsList.Count != discardPublishStates.Count)
            {
                throw new ResourceNotFoundException("Not all items could be located.", ErrorCodes.ItemNotFound);
            }

            var errorState = discardPublishStates.FirstOrDefault(dps => dps.NotExist);
            if (errorState != null)
            {
                throw new ResourceNotFoundException($"Item with ID {errorState.ItemId} is not found.", ErrorCodes.ItemNotFound);
            }

            errorState = discardPublishStates.FirstOrDefault(dps => dps.NotArtifact);
            if (errorState != null)
            {
                throw new ResourceNotFoundException($"Item with ID {errorState.ItemId} is not an artifact.", ErrorCodes.ItemNotFound);
            }

            errorState = discardPublishStates.FirstOrDefault(dps => dps.Deleted);
            if (errorState != null)
            {
                throw new ResourceNotFoundException($"Item with ID {errorState.ItemId} is deleted.", ErrorCodes.ItemNotFound);
            }

            errorState = discardPublishStates.FirstOrDefault(dps => dps.NoChanges);
            if (errorState != null)
            {
                throw new ConflictException($"Artifact with ID {errorState.ItemId} has nothing to publish. The artifact will now be refreshed.",
                    ErrorCodes.CannotPublish);
            }

            errorState = discardPublishStates.FirstOrDefault(dps => dps.Invalid);
            if (errorState != null)
            {
                throw new ConflictException($"Artifact with ID {errorState.ItemId} has validation errors.",
                    ErrorCodes.CannotPublishOverValidationErrors);
            }

            IDictionary<int, string> projectsNames = new Dictionary<int, string>();
            // These artifacts can be published alone.
            IList<int> independentArtifactsIds = discardPublishStates.Where(dps => !dps.PublishDependent).Select(dps => dps.ItemId).ToList();
            // These artifacts must be published together with other artifacts.
            IList<int> dependentArtifactsIds = discardPublishStates.Where(dps => dps.PublishDependent).Select(dps => dps.ItemId).ToList();

            if (dependentArtifactsIds.Any())
            {
                await ProcessDependentArtifactsDiscovery(parameters, dependentArtifactsIds, independentArtifactsIds, projectsNames);
            }

            Func<IDbTransaction, Task> action = async transaction =>
            {
                var publishRevision =
                    await
                        _sqlHelper.CreateRevisionInTransactionAsync(transaction, parameters.UserId, "New Publish: publishing artifacts.");
                var artifactIdsSet = artifactIdsList.ToHashSet();
                var publishResults = new List<SqlPublishResult>(artifactIdsSet.Count);
                var artifactStates = await _versionControlRepository.GetPublishStates(parameters.UserId, artifactIdsSet);

                var artifactsCannotBePublished = _versionControlRepository.CanPublish(artifactStates);

                artifactIdsSet.ExceptWith(artifactsCannotBePublished.Keys);

                // Notify about artifactsCannotBePublished - callback
                //if (onError != null && onError(new ReadOnlyDictionary<int, PublishErrors>(artifactsCannotBePublished)))
                //{
                //    throw new DataAccessException("Publish interrupted", BusinessLayerErrorCodes.RequiredArtifactHasNotBeenPublished);
                //}

                if (artifactIdsSet.Count == 0)
                {
                    return;
                }

                var env = new PublishEnvironment
                {
                    RevisionId = publishRevision,
                    Timestamp = DateTime.UtcNow, // Need to get if from created revision - DB timestamp
                    KeepLock = false,
                    ArtifactStates = artifactStates.ToDictionary(s => s.ItemId),
                    Repositories = null,
                    SensitivityCollector = new ReuseSensitivityCollector()
                };

                if (artifactIdsSet.Count > 0)
                {
                        var deletedArtifactIds = await _versionControlRepository.DetectAndPublishDeletedArtifacts(parameters.UserId, artifactIdsSet, env);
                        env.DeletedArtifactIds = deletedArtifactIds;
                        artifactIdsSet.ExceptWith(deletedArtifactIds);

                    ActionRepeater.Retry(() =>
                    {
                        _publishRepositoryComposer.Execute(_sqlHelper, publishRevision, TODO, TODO);
                    });

                    

                    publishResults.AddRange(env.GetChangeSqlPublishResults());
                }

                
            };

            var discardPublishDetails = (await _versionControlRepository.GetDiscardPublishDetails(parameters.UserId, artifactIdsList, true)).Details;
            return ToNovaArtifactResultSet(discardPublishDetails, projectsNames);
        }

        private async Task ProcessDependentArtifactsDiscovery(PublishParameters parameters, IList<int> dependentArtifactsIds,
            IList<int> independentArtifactsIds, IDictionary<int, string> projectsNames)
        {
            ICollection<int> publishDependentArtifacts = await _versionControlRepository.GetDiscardPublishDependentArtifacts(
                parameters.UserId, dependentArtifactsIds, false);
            if (publishDependentArtifacts.Any())
            {
                IEnumerable<int> unitedArtifactsIds =
                    publishDependentArtifacts.Union(dependentArtifactsIds).Union(independentArtifactsIds);
                var discardPublishDetails =
                    (await _versionControlRepository.GetDiscardPublishDetails(parameters.UserId, unitedArtifactsIds, true))
                        .Details;
                //We are supposed to throw an exception here for providing appropriate information to client
                throw new ConflictException("Specified artifacts have dependent artifacts to discard.",
                    ErrorCodes.CannotDiscardOverDependencies,
                    ToNovaArtifactResultSet(discardPublishDetails, projectsNames));
            }
        }

        private ArtifactResultSet ToNovaArtifactResultSet(ICollection<SqlDiscardPublishDetails> discardPublishDetails, IDictionary<int, string> projectsNames)
        {
            var artifactResultSet = new ArtifactResultSet();
            if (discardPublishDetails != null)
            {
                artifactResultSet.Artifacts = discardPublishDetails.Select(dpd => new Artifact
                {
                    Id = dpd.ItemId,
                    Name = dpd.Name,
                    ParentId = dpd.ParentId,
                    OrderIndex = dpd.OrderIndex,
                    ItemTypeId = dpd.ItemTypeId,
                    Prefix = dpd.ItemTypePrefix,
                    ItemTypeIconId = dpd.ItemTypeIconId,
                    PredefinedType = dpd.BaseItemTypePredefined,
                    ProjectId = dpd.ProjectId,
                    Version = dpd.VersionsCount == 0 ? -1 : dpd.VersionsCount
                }).ToList();

                SortArtifactsByProjectNameThenById(projectsNames, artifactResultSet);
            }
            if (projectsNames != null)
            {
                artifactResultSet.Projects = projectsNames.Select(pn => new Item
                {
                    Id = pn.Key,
                    Name = pn.Value
                }).ToList();
            }
            return artifactResultSet;
        }

        private static void SortArtifactsByProjectNameThenById(IDictionary<int, string> projectsNames,
            ArtifactResultSet novaArtifactResultSet)
        {
            novaArtifactResultSet.Artifacts =
                novaArtifactResultSet.Artifacts.OrderBy(a =>
                {
                    string projectName;
                    if (a.ProjectId <= 0)
                    {
                        return string.Empty;
                    }
                    projectsNames.TryGetValue(a.ProjectId, out projectName);
                    return projectName;
                }).ThenBy(a => a.Id).ToList();
        }
    }
}