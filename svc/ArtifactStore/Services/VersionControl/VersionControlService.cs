using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Models.VersionControl;
using ArtifactStore.Repositories.Revisions;
using ArtifactStore.Repositories.VersionControl;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Reuse;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Services.VersionControl
{
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

        public async Task<ArtifactResultSet> PublishArtifacts(PublishParameters parameters, IDbTransaction transaction = null)
        {
            IList<int> artifactIdsList;
            ICollection<SqlDiscardPublishState> discardPublishStates;

            if (parameters.All.HasValue && parameters.All.Value)
            {
                discardPublishStates = await _versionControlRepository.GetAllDiscardPublish(parameters.UserId, transaction);
                artifactIdsList = discardPublishStates.Select(dps => dps.ItemId).ToList();
            }
            else
            {
                artifactIdsList = (parameters.ArtifactIds ?? Enumerable.Empty<int>()).Distinct().ToList();
                if (!artifactIdsList.Any())
                {
                    throw new BadRequestException("The list of artifact Ids is empty.", ErrorCodes.IncorrectInputParameters);
                }
                discardPublishStates = await _versionControlRepository.GetDiscardPublishStates(parameters.UserId, artifactIdsList, transaction);
            }
            HandleErrorStates(artifactIdsList, discardPublishStates);

            IDictionary<int, string> projectsNames = new Dictionary<int, string>();
            // These artifacts can be published alone.
            IList<int> independentArtifactsIds = discardPublishStates.Where(dps => !dps.PublishDependent).Select(dps => dps.ItemId).ToList();
            // These artifacts must be published together with other artifacts.
            IList<int> dependentArtifactsIds = discardPublishStates.Where(dps => dps.PublishDependent).Select(dps => dps.ItemId).ToList();

            if (dependentArtifactsIds.Any())
            {
                await ProcessDependentArtifactsDiscovery(parameters, dependentArtifactsIds, independentArtifactsIds, projectsNames);
            }

            PublishEnvironment environment;

            if (transaction == null)
            {
                environment = await _sqlHelper.RunInTransactionAsync
                (
                    ServiceConstants.RaptorMain,
                    GetPublishTransactionAction(parameters, artifactIdsList)
                );
            }
            else
            {
                environment = await TransactionalPublishArtifact(parameters, artifactIdsList, transaction);
            }

            var discardPublishDetailsResult =
                await _versionControlRepository.GetDiscardPublishDetails(parameters.UserId, artifactIdsList, true, transaction);
            var discardPublishDetails = discardPublishDetailsResult.Details;
            projectsNames = discardPublishDetailsResult.ProjectInfos;
            var artifactResultSet = ToNovaArtifactResultSet(discardPublishDetails, environment, projectsNames);
            artifactResultSet.RevisionId = parameters.RevisionId;
            return artifactResultSet;
        }

        private static void HandleErrorStates(IList<int> artifactIdsList, ICollection<SqlDiscardPublishState> discardPublishStates)
        {
            if (artifactIdsList.Count != discardPublishStates.Count)
            {
                throw new ResourceNotFoundException(I18NHelper.FormatInvariant("Not all items could be located."), ErrorCodes.ItemNotFound);
            }

            var errorState = discardPublishStates.FirstOrDefault(dps => dps.NotExist);
            if (errorState != null)
            {
                throw new ResourceNotFoundException(I18NHelper.FormatInvariant("Item with ID {0} is not found.", errorState.ItemId),
                    ErrorCodes.ItemNotFound);
            }

            errorState = discardPublishStates.FirstOrDefault(dps => dps.NotArtifact);
            if (errorState != null)
            {
                throw new ResourceNotFoundException(I18NHelper.FormatInvariant("Item with ID {0} is not an artifact.", errorState.ItemId),
                    ErrorCodes.ItemNotFound);
            }

            errorState = discardPublishStates.FirstOrDefault(dps => dps.Deleted);
            if (errorState != null)
            {
                throw new ResourceNotFoundException(I18NHelper.FormatInvariant("Item with ID {0} is deleted.", errorState.ItemId),
                    ErrorCodes.ItemNotFound);
            }

            errorState = discardPublishStates.FirstOrDefault(dps => dps.NoChanges);
            if (errorState != null)
            {
                throw new ConflictException(
                    I18NHelper.FormatInvariant("Artifact with ID {0} has nothing to publish. The artifact will now be refreshed.",
                    errorState.ItemId),
                    ErrorCodes.CannotPublish);
            }

            errorState = discardPublishStates.FirstOrDefault(dps => dps.Invalid);
            if (errorState != null)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact with ID {0} has validation errors.", errorState.ItemId),
                    ErrorCodes.CannotPublishOverValidationErrors);
            }
        }

        private Func<IDbTransaction, Task<PublishEnvironment>> GetPublishTransactionAction(PublishParameters parameters, IList<int> artifactIdsList)
        {
            Func<IDbTransaction, Task<PublishEnvironment>> action = async transaction =>
            await TransactionalPublishArtifact(parameters, artifactIdsList, transaction);

            return action;
        }

        private async Task<PublishEnvironment> TransactionalPublishArtifact(PublishParameters parameters,
            IList<int> artifactIdsList,
            IDbTransaction transaction)
        {
            int publishRevision = parameters.RevisionId ?? await
                        _sqlHelper.CreateRevisionInTransactionAsync(transaction, parameters.UserId,
                            "New Publish: publishing artifacts.");
            parameters.RevisionId = publishRevision;
            parameters.AffectedArtifactIds.Clear();
            parameters.AffectedArtifactIds.AddRange(artifactIdsList);
            var publishResults = new List<SqlPublishResult>(parameters.AffectedArtifactIds.Count);
            var artifactStates =
                await _versionControlRepository.GetPublishStates(parameters.UserId, parameters.AffectedArtifactIds, transaction: transaction);

            var artifactsCannotBePublished = _versionControlRepository.CanPublish(artifactStates);

            parameters.AffectedArtifactIds.ExceptWith(artifactsCannotBePublished.Keys);

            // Notify about artifactsCannotBePublished - callback
            //if (onError != null && onError(new ReadOnlyDictionary<int, PublishErrors>(artifactsCannotBePublished)))
            //{
            //    throw new DataAccessException("Publish interrupted", BusinessLayerErrorCodes.RequiredArtifactHasNotBeenPublished);
            //}

            if (parameters.AffectedArtifactIds.Count == 0)
            {
                return null;
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

            if (parameters.AffectedArtifactIds.Count <= 0)
            {
                return env;
            }

            env.DeletedArtifactIds.Clear();
            env.DeletedArtifactIds.AddRange(
                await _versionControlRepository.DetectAndPublishDeletedArtifacts(parameters.UserId,
                    parameters.AffectedArtifactIds,
                    env, transaction));
            parameters.AffectedArtifactIds.ExceptWith(env.DeletedArtifactIds);

            //Release lock
            if (!env.KeepLock)
            {
                await _versionControlRepository.ReleaseLock(parameters.UserId, parameters.AffectedArtifactIds, transaction);
            }

            await _publishRepositoryComposer.Execute(publishRevision, parameters, env, transaction);

            //Add history
            await _revisionRepository.AddHistory(publishRevision, parameters.AffectedArtifactIds, transaction);
            //});

            publishResults.AddRange(env.GetChangeSqlPublishResults());

            return env;
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
                    ToNovaArtifactResultSet(discardPublishDetails, null, projectsNames));
            }
        }

        private ArtifactResultSet ToNovaArtifactResultSet(ICollection<SqlDiscardPublishDetails> discardPublishDetails, PublishEnvironment environment, IDictionary<int, string> projectsNames)
        {
            var artifactResultSet = new ArtifactResultSet();
            if (discardPublishDetails != null)
            {
                artifactResultSet.Artifacts.Clear();
                foreach (var dpd in discardPublishDetails)
                {
                    var artifact = new Artifact
                    {
                        Id = dpd.ItemId,
                        Name = dpd.Name,
                        ParentId = dpd.ParentId,
                        OrderIndex = dpd.OrderIndex,
                        ItemTypeId = dpd.ItemType_ItemTypeId,
                        Prefix = dpd.Prefix,
                        ItemTypeIconId = dpd.Icon_ImageId,
                        PredefinedType = dpd.PrimitiveItemTypePredefined,
                        ProjectId = dpd.VersionProjectId,
                        Version = dpd.VersionsCount == 0 ? -1 : dpd.VersionsCount
                    };
                    if (environment != null && environment.SensitivityCollector.ArtifactModifications != null)
                    {
                        ReuseSensitivityCollector.ArtifactModification artifactModification;
                        if (environment.SensitivityCollector.ArtifactModifications.TryGetValue(artifact.Id, out artifactModification) 
                            && artifactModification?.ModifiedPropertyTypes != null)
                        {
                            artifactResultSet.ModifiedProperties.Add(artifact.Id, new List<Property>());
                            artifactResultSet.ModifiedProperties[artifact.Id].AddRange(
                                artifactModification.ModifiedPropertyTypes.Select(p => new Property
                                {
                                    PropertyTypeId = p.Item1,
                                    Predefined = p.Item2
                                }));
                        }
                    }
                    artifactResultSet.Artifacts.Add(artifact);
                }
                SortArtifactsByProjectNameThenById(projectsNames, artifactResultSet);
            }
            if (projectsNames != null)
            {
                artifactResultSet.Projects.Clear();
                artifactResultSet.Projects.AddRange(projectsNames.Select(pn => new Item
                {
                    Id = pn.Key,
                    Name = pn.Value
                }).ToList());
            }
            return artifactResultSet;
        }

        private static void SortArtifactsByProjectNameThenById(IDictionary<int, string> projectsNames,
            ArtifactResultSet novaArtifactResultSet)
        {
            var artifactResultSet = novaArtifactResultSet.Artifacts.ToArray();
            novaArtifactResultSet.Artifacts.Clear();
            novaArtifactResultSet.Artifacts.AddRange(
                artifactResultSet.OrderBy(a =>
                {
                    string projectName;
                    if (a.ProjectId <= 0)
                    {
                        return string.Empty;
                    }
                    projectsNames.TryGetValue(a.ProjectId, out projectName);
                    return projectName;
                }).ThenBy(a => a.Id));
        }
    }
}