using Common;
using Model.Factories;
using Model.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Model.OpenApiModel.Services;
using Utilities;
using Utilities.Facades;

namespace Model.ArtifactModel.Impl
{
    /// <summary>
    /// This is returned by OpenAPI when adding an artifact.
    /// Currently I can't see a way for these to ever be set to true.
    /// </summary>
    public class ArtifactStatus
    {
        public bool IsLocked { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public class ArtifactBase : IArtifactBase, IArtifactObservable
    {
        #region Constants

        public const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";

        #endregion Constants

        #region Properties

        [JsonIgnore]
        public bool ShouldDeleteChildren { get; set; }
        [JsonIgnore]
        public IUser LockOwner { get; set; }
        [JsonIgnore]
        public string Address { get; set; }
        [JsonIgnore]
        public IUser CreatedBy { get; set; }
        [JsonIgnore]
        public bool IsPublished { get; set; }
        [JsonIgnore]
        public bool IsSaved { get; set; }
        [JsonIgnore]
        public bool IsMarkedForDeletion { get; set; }
        [JsonIgnore]
        public bool IsDeleted { get; set; }
        [JsonIgnore]
        public IProject Project { get; set; }

        [JsonIgnore]
        public bool AreTracesReadOnly { get; set; }
        [JsonIgnore]
        public bool AreAttachmentsReadOnly { get; set; }
        [JsonIgnore]
        public bool AreDocumentReferencesReadOnly { get; set; }

        #region Serialized JSON Properties

        public BaseArtifactType BaseArtifactType { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public int Version { get; set; }
        public int ParentId { get; set; }
        public Uri BlueprintUrl { get; set; }
        public int ArtifactTypeId { get; set; }
        public string ArtifactTypeName { get; set; }
        public ArtifactStatus Status { get; set; } = new ArtifactStatus();

        //TODO  Check if we can remove the setters and get rid of these warnings

        //TODO Remove these from here or make them generic for both Artifact and OpenApiArtifact (So we don't need to use OpenApiArtifact in the Artifact class

        [JsonConverter(typeof (SerializationUtilities.ConcreteConverter<List<OpenApiProperty>>))]
        public List<OpenApiProperty> Properties { get; set; } = new List<OpenApiProperty>();

        #endregion Serialized JSON Properties
        #endregion Properties

        #region Constructors
        
        /// <summary>
        /// Constructor needed to deserialize it as generic type.
        /// </summary>
        public ArtifactBase()
        {
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address">The URI address of the artifact.</param>
        public ArtifactBase(string address) : this()
        {
            ThrowIf.ArgumentNull(address, nameof(address));
            Address = address;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address">The base url of the API</param>
        /// <param name="id">The artifact id</param>
        /// <param name="projectId">The project containing the artifact</param>
        public ArtifactBase(string address, int id, int projectId) : this(address)
        {
            Id = id;
            ProjectId = projectId;
        }

        #endregion Constructors

        #region Implements IDeepCopyable

        /// <seealso cref="IDeepCopyable{T}.DeepCopy()"/>
        public IArtifactBase DeepCopy()
        {
            var artifactBase = new ArtifactBase
            {
                ShouldDeleteChildren = this.ShouldDeleteChildren,
                LockOwner = this.LockOwner,
                Address = this.Address,
                CreatedBy = this.CreatedBy,
                IsPublished = this.IsPublished,
                IsSaved = this.IsSaved,
                IsMarkedForDeletion = this.IsMarkedForDeletion,
                IsDeleted = this.IsDeleted,

                Id = this.Id,
                Name = this.Name,
                ProjectId = this.ProjectId,
                Version = this.Version,
                ParentId = this.ParentId,
                BlueprintUrl = this.BlueprintUrl,
                ArtifactTypeId = this.ArtifactTypeId,
                ArtifactTypeName = this.ArtifactTypeName,
                BaseArtifactType = this.BaseArtifactType,
                AreTracesReadOnly = this.AreTracesReadOnly,
                AreAttachmentsReadOnly = this.AreAttachmentsReadOnly,
                AreDocumentReferencesReadOnly = this.AreDocumentReferencesReadOnly,
                
                Project = this.Project,
                Status = this.Status
            };

            DeletedArtifactResults.AddRange(this.DeletedArtifactResults);
            Properties.AddRange(this.Properties);

            return artifactBase;
        }

        #endregion Implements IDeepCopyable

        #region Delete methods

        public List<OpenApiDeleteArtifactResult> DeletedArtifactResults { get; } = new List<OpenApiDeleteArtifactResult>();

        /// <seealso cref="IArtifactBase.Delete(IUser, bool?, List{HttpStatusCode})"/>
        public virtual List<OpenApiDeleteArtifactResult> Delete(IUser user = null,
            bool? deleteChildren = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform Delete.");
                user = CreatedBy;
            }

            var deleteArtifactResults = OpenApi.DeleteArtifact(Address, this, user,
                deleteChildren: deleteChildren ?? ShouldDeleteChildren,
                expectedStatusCodes: expectedStatusCodes);

            UpdateStateOfDeletedArtifacts(deleteArtifactResults, this, user, Address);

            return deleteArtifactResults;
        }

        /// <summary>
        /// Updates the internal flags of all artifacts affected by the deleted artifact to indicate they are now marked for deletion.
        /// </summary>
        /// <param name="artifactResults">The results of the OpenAPI delete artifact call.</param>
        /// <param name="artifactToDelete">The main artifact that was deleted.</param>
        /// <param name="user">The user who deleted the artifact(s).</param>
        /// <param name="path">The path of the delete REST call.</param>
        private static void UpdateStateOfDeletedArtifacts(List<OpenApiDeleteArtifactResult> artifactResults,
            IArtifactBase artifactToDelete,
            IUser user,
            string path)
        {
            var artifaceBaseToDelete = artifactToDelete as ArtifactBase;

            foreach (var deletedArtifactResult in artifactResults)
            {
                Logger.WriteDebug("DELETE {0} returned following: ArtifactId: {1} Message: {2}, ResultCode: {3}",
                    path, deletedArtifactResult.ArtifactId, deletedArtifactResult.Message, deletedArtifactResult.ResultCode);

                if (deletedArtifactResult.ResultCode == HttpStatusCode.OK)
                {
                    artifaceBaseToDelete.DeletedArtifactResults.Add(deletedArtifactResult);

                    if (deletedArtifactResult.ArtifactId == artifactToDelete.Id)
                    {
                        if (artifactToDelete.IsPublished)
                        {
                            artifactToDelete.IsMarkedForDeletion = true;
                            artifaceBaseToDelete.LockOwner = user;
                        }
                        else
                        {
                            artifactToDelete.IsDeleted = true;
                        }
                    }
                }
            }
        }

        #endregion Delete methods

        #region Navigation methods

        /// <seealso cref="IArtifactBase.GetNavigation(IUser, List{IArtifact}, int?, int?, int?, bool?, List{HttpStatusCode})"/>
        public List<ArtifactReference> GetNavigation(
            IUser user,
            List<IArtifact> artifacts,
            int? versionId = null,
            int? revisionId = null,
            int? baselineId = null,
            bool? readOnly = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetNavigation(Address, user, artifacts,
                versionId: versionId,
                revisionId: revisionId,
                baselineId: baselineId,
                readOnly: readOnly,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <summary>
        /// Get ArtifactReference list which is used to represent breadcrumb navigation.
        /// (Runs:  'GET /svc/shared/navigation/{id1}/{id2}...')
        /// </summary>
        /// <param name="address">The base URL of the Blueprint server.</param>
        /// <param name="user">The user credentials for breadcrumb navigation.</param>
        /// <param name="artifacts">The list of artifacts used for breadcrumb navigation.</param>
        /// <param name="versionId">(optional) The Version ID??</param>
        /// <param name="revisionId">(optional) The Revision ID??</param>
        /// <param name="baselineId">(optional) The Baseline ID??</param>
        /// <param name="readOnly">(optional) Indicator which determines if returning artifact references are readOnly or not.
        ///     By default, readOnly is set to false.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.</param>
        /// <returns>The List of ArtifactReferences after the get navigation call.</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<ArtifactReference> GetNavigation(
            string address,
            IUser user,
            List<IArtifact> artifacts,
            int? versionId = null,
            int? revisionId = null,
            int? baselineId = null,
            bool? readOnly = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return SvcShared.GetNavigation(address, user, artifacts,
                versionId: versionId,
                revisionId: revisionId,
                baselineId: baselineId,
                readOnly: readOnly,
                expectedStatusCodes: expectedStatusCodes);
        }

        #endregion Navigation methods

        #region Publish methods

        /// <seealso cref="IArtifactBase.Publish(IUser, bool, List{HttpStatusCode})"/>
        public virtual void Publish(IUser user = null,
            bool shouldKeepLock = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform Publish.");
                user = CreatedBy;
            }

            var artifactToPublish = new List<IArtifactBase> { this };

            PublishArtifacts(artifactToPublish, Address, user, shouldKeepLock, expectedStatusCodes);
        }

        /// <summary>
        /// Publish Artifact(s) (Used when publishing a single artifact OR a list of artifacts)
        /// </summary>
        /// <param name="artifactsToPublish">The list of artifacts to publish</param>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="shouldKeepLock">(optional) Boolean parameter which defines whether or not to keep the lock after publishing the artfacts</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only '200 OK' is expected.</param>
        /// <returns>The list of OpenApiPublishArtifactResult objects created by the publish artifacts request.</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<OpenApiPublishArtifactResult> PublishArtifacts(List<IArtifactBase> artifactsToPublish,
            string address,
            IUser user,
            bool shouldKeepLock = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactsToPublish, nameof(artifactsToPublish));

            var publishedResultList = OpenApi.PublishArtifacts(artifactsToPublish, address, user, shouldKeepLock, expectedStatusCodes);

            var deletedArtifactsList = new List<IArtifactBase>();

            // When each artifact is successfully published, set IsSaved flag to false since there are no longer saved changes
            foreach (var publishedResult in publishedResultList)
            {
                Logger.WriteDebug("Result Code for the Published Artifact {0}: {1}", publishedResult.ArtifactId, publishedResult.ResultCode);

                var publishedArtifact = artifactsToPublish.Find(a => a.Id.Equals(publishedResult.ArtifactId));

                if (publishedResult.ResultCode == HttpStatusCode.OK)
                {
                    publishedArtifact.LockOwner = null;
                    publishedArtifact.IsSaved = false;

                    // If the artifact was marked for deletion, then this publish operation actually deleted the artifact.
                    if (publishedArtifact.IsMarkedForDeletion)
                    {
                        deletedArtifactsList.Add(publishedArtifact);

                        publishedArtifact.IsPublished = false;
                        publishedArtifact.IsDeleted = true;
                    }
                    else
                    {
                        publishedArtifact.IsPublished = true;
                    }
                }
            }

            if (deletedArtifactsList.Any())
            {
                deletedArtifactsList[0]?.NotifyArtifactDeleted(deletedArtifactsList);
            }

            Assert.That(publishedResultList.Count.Equals(artifactsToPublish.Count),
                "The number of artifacts passed for Publish was {0} but the number of artifacts returned was {1}",
                artifactsToPublish.Count, publishedResultList.Count);

            return publishedResultList;
        }

        #endregion Publish methods

        #region IArtifactObservable methods

        [JsonIgnore]
        public List<IArtifactObserver> ArtifactObservers { get; private set; }

        /// <seealso cref="RegisterObserver(IArtifactObserver)"/>
        public void RegisterObserver(IArtifactObserver observer)
        {
            if (ArtifactObservers == null)
            {
                ArtifactObservers = new List<IArtifactObserver>();
            }

            ArtifactObservers.Add(observer);
        }

        /// <seealso cref="UnregisterObserver(IArtifactObserver)"/>
        public void UnregisterObserver(IArtifactObserver observer)
        {
            ArtifactObservers?.Remove(observer);
        }

        /// <seealso cref="NotifyArtifactDeleted(List{IArtifactBase})"/>
        public void NotifyArtifactDeleted(List<IArtifactBase> deletedArtifactsList)
        {
            ThrowIf.ArgumentNull(deletedArtifactsList, nameof(deletedArtifactsList));

            // Notify the observers about any artifacts that were deleted as a result of this publish.
            foreach (var deletedArtifact in deletedArtifactsList)
            {
                IEnumerable<int> deletedArtifactIds =
                    from result in ((ArtifactBase)deletedArtifact).DeletedArtifactResults
                    select result.ArtifactId;

                Logger.WriteDebug("*** Notifying observers about deletion of artifact IDs: {0}", string.Join(", ", deletedArtifactIds));
                deletedArtifact.ArtifactObservers?.ForEach(o => o.NotifyArtifactDeleted(deletedArtifactIds));
            }
        }

        /// <seealso cref="NotifyArtifactPublished(List{INovaArtifactResponse})"/>
        public void NotifyArtifactPublished(List<INovaArtifactResponse> publishedArtifactsList)
        {
            ThrowIf.ArgumentNull(publishedArtifactsList, nameof(publishedArtifactsList));

            // Notify the observers about any artifacts that were deleted as a result of this publish.
            IEnumerable<int> publishedArtifactIds =
                from result in publishedArtifactsList
                select result.Id;

            Logger.WriteDebug("*** Notifying observers about publish of artifact IDs: {0}", string.Join(", ", publishedArtifactIds));
            ArtifactObservers?.ForEach(o => o.NotifyArtifactPublished(publishedArtifactIds));
        }

        #endregion IArtifactObservable methods

        /// <summary>
        /// Replace properties in an artifact with properties from another artifact
        /// </summary>
        /// <param name="sourceArtifactBase">The artifact that is the source of the properties</param>
        /// <param name="destinationArtifactBase">The artifact that is the destination of the properties</param>
        public static void ReplacePropertiesWithPropertiesFromSourceArtifact(IArtifactBase sourceArtifactBase, IArtifactBase destinationArtifactBase)
        {
            ThrowIf.ArgumentNull(sourceArtifactBase, nameof(sourceArtifactBase));
            ThrowIf.ArgumentNull(destinationArtifactBase, nameof(destinationArtifactBase));

            // List of properties not to be replaced by the source artifact properties
            var propertiesNotToBeReplaced = new List<string>
            {
                // Can't be replaced from destination because our IUser model differs from the Blueprint implementation
                "CreatedBy",
                // This needs to be maintained so that it is not overwritten with null
                "Address",
                // Need to figure out what this method does. Without having Project in this list it set Project to null.
                "Project"
            };

            foreach (PropertyInfo sourcePropertyInfo in sourceArtifactBase.GetType().GetProperties())
            {
                PropertyInfo destinationPropertyInfo = destinationArtifactBase.GetType().GetProperties().FirstOrDefault(p => p.Name == sourcePropertyInfo.Name);

                if (destinationPropertyInfo != null && destinationPropertyInfo.CanWrite && !propertiesNotToBeReplaced.Contains(destinationPropertyInfo.Name))
                {
                    var value = sourcePropertyInfo.GetValue(sourceArtifactBase);
                    destinationPropertyInfo.SetValue(destinationArtifactBase, value);
                }
            }
        }


        /// <summary>
        /// Replace properties in an artifact with properties from an ArtifactDetails object.
        /// </summary>
        /// <param name="sourceArtifact">The ArtifactDetails to copy properties from.</param>
        /// <param name="project">The project where the artifact exists.</param>
        /// <param name="user">A user to authenticate with.</param>
        /// <param name="skipNullValues">(optional) If true (the default) it doesn't replace properties if the value of the sourceArtifact property is null.</param>
        public void ReplacePropertiesWithPropertiesFromSourceArtifactDetails(INovaArtifactDetails sourceArtifact,
            IProject project,
            IUser user,
            bool skipNullValues = true)
        {
            ThrowIf.ArgumentNull(sourceArtifact, nameof(sourceArtifact));

            Id = sourceArtifact.Id;
            Name = skipNullValues ? sourceArtifact.Name ?? Name : sourceArtifact.Name;
            ParentId = sourceArtifact.ParentId.Value;
            ProjectId = sourceArtifact.ProjectId.Value;
            Version = sourceArtifact.Version.Value;

            // Now set the list of Property objects.
            AddOrReplaceTextOrChoiceValueProperty("Name", Name, project, user, skipNullValues);
            AddOrReplaceTextOrChoiceValueProperty(nameof(NovaArtifactDetails.Description), sourceArtifact.Description, project, user, skipNullValues);

            // TODO: There is also an "ID" property to set which has a value of <prefix> + <Id>.  Ex. "AC1503".

            if (sourceArtifact.CreatedOn != null)
            {
                AddOrReplaceDateValueProperty("Created On", sourceArtifact.CreatedOn, project, user, skipNullValues);
            }

            if (sourceArtifact.LastEditedOn != null)
            {
                AddOrReplaceDateValueProperty("Last Edited On", sourceArtifact.LastEditedOn, project, user, skipNullValues);
            }

            AddOrReplaceUsersAndGroupsProperty("Created By", sourceArtifact.CreatedBy, project, user);
            AddOrReplaceUsersAndGroupsProperty("Last Edited By", sourceArtifact.LastEditedBy, project, user);

            // TODO: Convert CustomProperties & SpecificPropertyValues into OpenApiProperty lists and set them...
            //CustomProperties.AddRange(sourceArtifact.CustomProperties);
            //SpecificPropertyValues.AddRange(sourceArtifact.SpecificPropertyValues);
        }

        /// <summary>
        /// Add or replace the specified TextOrChoiceValue property value with a new value.
        /// </summary>
        /// <param name="propertyName">The name of the property in the OpenApiArtifact.</param>
        /// <param name="propertyValue">The new value to set.</param>
        /// <param name="project">The project where this artifact exists.</param>
        /// <param name="user">A user to authenticate with.</param>
        /// <param name="skipNullValues">(optional) Pass true to only replace if the value is not null.</param>
        public void AddOrReplaceTextOrChoiceValueProperty(string propertyName,
            string propertyValue,
            IProject project,
            IUser user,
            bool skipNullValues = false)
        {
            if (skipNullValues && (propertyValue == null))
            {
                return;
            }

            var property = Properties.Find(p => p.Name == propertyName);

            if (property == null)
            {
                property = OpenApiProperty.SetPropertyAttribute(Address, project, user, this.BaseArtifactType, propertyName, propertyValue);
                Properties.Add(property);
            }

            property.TextOrChoiceValue = propertyValue;
        }

        /// <summary>
        /// Add or replace the specified DateValue property value with a new value.
        /// </summary>
        /// <param name="propertyName">The name of the property in the OpenApiArtifact.</param>
        /// <param name="propertyValue">The new value to set.</param>
        /// <param name="project">The project where this artifact exists.</param>
        /// <param name="user">A user to authenticate with.</param>
        /// <param name="skipNullValues">(optional) Pass true to only replace if the value is not null.</param>
        private void AddOrReplaceDateValueProperty(string propertyName,
            DateTime? propertyValue,
            IProject project,
            IUser user,
            bool skipNullValues = false)
        {
            // TODO: See if this function can be replaced by something similar to SetProperty<T>() in ArtifactStoreTests.SaveArtifactTests.
            
            if (skipNullValues && (propertyValue == null))
            {
                return;
            }

            var property = Properties.Find(p => p.Name == propertyName);

            if (property == null)
            {
                property = OpenApiProperty.SetPropertyAttribute(Address, project, user, this.BaseArtifactType, propertyName, dateValue: propertyValue);
                Properties.Add(property);
            }

            property.DateValue = propertyValue.ToStringInvariant();
        }

        /// <summary>
        /// Add or replace the specified UsersAndGroups property value with a new value.
        /// </summary>
        /// <param name="propertyName">The name of the property in the OpenApiArtifact.</param>
        /// <param name="propertyValue">The new value to set.</param>
        /// <param name="project">The project where this artifact exists.</param>
        /// <param name="user">A user to authenticate with.</param>
        /// <param name="userType">(optional) Specify whether the value being added is a user or group.</param>
        private void AddOrReplaceUsersAndGroupsProperty(string propertyName,
            Identification propertyValue,
            IProject project,
            IUser user,
            UsersAndGroupsType userType = UsersAndGroupsType.User)
        {
            // Only add/replace if the source artifact had a value for this property.
            if (propertyValue != null)
            {
                var property = Properties.Find(p => p.Name == propertyName);

                var userOrGroup = new UsersAndGroups
                {
                    Type = userType,
                    DisplayName = propertyValue.DisplayName,
                    Id = propertyValue.Id
                };

                var usersAndGroups = new List<UsersAndGroups> {userOrGroup};

                if (property == null)
                {
                    property = OpenApiProperty.SetPropertyAttribute(Address, project, user, this.BaseArtifactType, propertyName,
                        usersAndGroups: usersAndGroups);
                    Properties.Add(property);
                }

                property.UsersAndGroups?.Clear();
                property.UsersAndGroups?.Add(userOrGroup);
            }
        }

        /// <summary>
        /// A helper function to dispose a list of artifacts.  Call this inside Dispose() methods of objects containing artifacts.
        /// </summary>
        /// <param name="artifactList">The list of artifacts to dispose.</param>
        /// <param name="observer">The observer to notify when artifacts are disposed.</param>
        public static void DisposeArtifacts(List<IArtifactBase> artifactList, IArtifactObserver observer)
        {
            if ((artifactList == null) || !artifactList.Any())
            {
                return;
            }

            var artifactStore = ArtifactStoreFactory.GetArtifactStoreFromTestConfig();
            var savedArtifactsDictionary = new Dictionary<IUser, List<IArtifactBase>>();
            var publishedArtifactsDictionary = new Dictionary<IUser, List<IArtifactBase>>();

            // Create lists of artifacts that are saved vs published by user.
            foreach (var artifact in artifactList.ToArray())
            {
                artifactList.Remove(artifact);
                var user = artifact.LockOwner ?? artifact.CreatedBy;

                if (artifact.IsDeleted)
                {
                    Logger.WriteDebug("Artifact already deleted.  Skipping deletion in the Dispose.");
                }
                else if (artifact.IsPublished)
                {
                    if (publishedArtifactsDictionary.ContainsKey(user))
                    {
                        publishedArtifactsDictionary[user].Add(artifact);
                    }
                    else
                    {
                        publishedArtifactsDictionary.Add(user, new List<IArtifactBase> { artifact });
                    }
                }
                else
                {
                    if (savedArtifactsDictionary.ContainsKey(user))
                    {
                        savedArtifactsDictionary[user].Add(artifact);
                    }
                    else
                    {
                        savedArtifactsDictionary.Add(user, new List<IArtifactBase> { artifact });
                    }
                }

                artifact.UnregisterObserver(observer);
            }

            // For each user that created saved artifacts, discard the list of artifacts they created.
            foreach (IUser user in savedArtifactsDictionary.Keys)
            {
                Logger.WriteDebug("*** Discarding all unpublished artifacts created by user: '{0}'.", user.Username);
                artifactStore.DiscardAllArtifacts(user);
            }

            // Create & authenticate an admin user that has access to delete all artifacts.
            var adminUser = UserFactory.CreateUserAndAddToDatabase();
            var adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            var openApi = BlueprintServerFactory.GetBlueprintServerFromTestConfig();

            try
            {
                adminStore.AddSession(adminUser);
                openApi.LoginUsingBasicAuthorization(adminUser);

                // For each user that created published artifacts, delete & publish the list of artifacts they created.
                foreach (var user in publishedArtifactsDictionary.Keys)
                {
                    // First discard all to release any locks by the original user.
                    Logger.WriteDebug("*** Discarding all artifacts created by user: '{0}'.", user.Username);
                    artifactStore.DiscardAllArtifacts(user);

                    // Now delete all artifacts using the Admin user.
                    var artifacts = publishedArtifactsDictionary[user];
                    Logger.WriteDebug("*** Deleting all published artifacts created by user: '{0}'.", user.Username);

                    foreach (var artifact in artifacts)
                    {
                        DeleteArtifact(artifact, adminUser);
                    }

                    Logger.WriteDebug("*** Publishing all deleted artifacts that were created by user: '{0}'...", user.Username);
                    artifactStore.PublishArtifacts(artifacts, adminUser, publishAll: true);
                }
            }
            finally
            {
                adminStore.DeleteSession(adminUser);
                adminUser.DeleteUser();
            }
        }

        /// <summary>
        /// Delete the artifact (if not already marked for deletion) using the specified user.
        /// </summary>
        /// <param name="artifact">The artifact to delete.</param>
        /// <param name="user">The user to authenticate with.</param>
        private static void DeleteArtifact(IArtifactBase artifact, IUser user)
        {
            if (!artifact.IsMarkedForDeletion && artifact.IsPublished)
            {
                // TODO: See if we can give the user Delete permission if they don't already have it.
                Logger.WriteDebug("Deleting artifact ID: {0}, and its children for user: '{1}'.", artifact.Id, user.Username);
                var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK, HttpStatusCode.NotFound };
                artifact.Delete(user, deleteChildren: true, expectedStatusCodes: expectedStatusCodes);
            }
        }
    }

    public static class ArtifactValidationMessage
    {
        public static readonly string ArtifactAlreadyLocked = "The artifact is locked by other user.";
        public static readonly string ArtifactAlreadyPublished = "Artifact {0} is already published in the project";
        public static readonly string ArtifactNothingToDiscard = "Artifact {0} has nothing to discard";
    }
}
