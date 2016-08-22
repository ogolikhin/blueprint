using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using NUnit.Framework;
using System.Reflection;
using Common;
using Model.Impl;
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

        public bool ShouldDeleteChildren { get; set; }
        public IUser LockOwner { get; set; }
        public string Address { get; set; }
        public IUser CreatedBy { get; set; }
        public bool IsPublished { get; set; }
        public bool IsSaved { get; set; }
        public bool IsMarkedForDeletion { get; set; }
        public bool IsDeleted { get; set; }
        public IProject Project { get; set; }

        public bool AreTracesReadOnly { get; set; }
        public bool AreAttachmentsReadOnly { get; set; }
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
        public ArtifactStatus Status { get; set; }

        //TODO  Check if we can remove the setters and get rid of these warnings

        //TODO Remove these from here or make them generic for both Artifact and OpenApiArtifact (So we don't need to use OpenApiArtifact in the Artifact class

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof (Deserialization.ConcreteConverter<List<OpenApiProperty>>))]
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
            IArtifactBase artifactBase = new ArtifactBase
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
                AreTracesReadOnly = this.AreTracesReadOnly,
                AreAttachmentsReadOnly = this.AreAttachmentsReadOnly,
                AreDocumentReferencesReadOnly = this.AreDocumentReferencesReadOnly
            };

            Properties.AddRange(this.Properties);

            return artifactBase;
        }

        #endregion Implements IDeepCopyable

        #region Delete methods

        protected List<DeleteArtifactResult> DeletedArtifactResults { get; private set; }

        public virtual List<DeleteArtifactResult> Delete(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false,
            bool? deleteChildren = null)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform Delete.");
                user = CreatedBy;
            }

            var deleteArtifactResults = DeleteArtifact(
                this,
                user,
                expectedStatusCodes,
                sendAuthorizationAsCookie,
                deleteChildren ?? ShouldDeleteChildren);

            return deleteArtifactResults;
        }

        /// <summary>
        /// Delete a single artifact on Blueprint server.
        /// To delete artifact permanently, Publish must be called after the Delete, otherwise the deletion can be discarded.
        /// </summary>
        /// <param name="artifactToDelete">The list of artifacts to delete</param>
        /// <param name="user">The user deleting the artifact. If null, attempts to delete using the credentials
        /// of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <param name="deleteChildren">(optional) Specifies whether or not to also delete all child artifacts of the specified artifact</param>
        /// <returns>The DeletedArtifactResult list after delete artifact call</returns>
        public static List<DeleteArtifactResult> DeleteArtifact(IArtifactBase artifactToDelete,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false,
            bool? deleteChildren = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactToDelete, nameof(artifactToDelete));

            string tokenValue = user.Token?.OpenApiToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.ARTIFACTS_id_, artifactToDelete.ProjectId, artifactToDelete.Id);

            var queryparameters = new Dictionary<string, string>();

            if (deleteChildren ?? artifactToDelete.ShouldDeleteChildren)
            {
                Logger.WriteDebug("*** Recursively deleting children for artifact ID: {0}.", artifactToDelete.Id);
                queryparameters.Add("Recursively", "true");
            }

            RestApiFacade restApi = new RestApiFacade(artifactToDelete.Address, tokenValue);
            var artifactResults = restApi.SendRequestAndDeserializeObject<List<DeleteArtifactResult>>(
                path,
                RestRequestMethod.DELETE,
                queryParameters: queryparameters,
                expectedStatusCodes: expectedStatusCodes);

            ArtifactBase artifaceBaseToDelete = artifactToDelete as ArtifactBase;
            artifaceBaseToDelete.DeletedArtifactResults = new List<DeleteArtifactResult>();

            foreach (var deletedArtifactResult in artifactResults)
            {
                Logger.WriteDebug("DELETE {0} returned following: ArtifactId: {1} Message: {2}, ResultCode: {3}",
                    path, deletedArtifactResult.ArtifactId, deletedArtifactResult.Message, deletedArtifactResult.ResultCode);

                if (deletedArtifactResult.ResultCode == HttpStatusCode.OK)
                {
                    artifaceBaseToDelete.DeletedArtifactResults.Add(deletedArtifactResult);

                    if (deletedArtifactResult.ArtifactId == artifactToDelete.Id)
                    {
                        artifactToDelete.IsMarkedForDeletion = true;
                    }
                }
            }

            // TODO:  Add an IsMarkedForDeletion flag to this class and set it to true if the delete was successful,
            // TODO:  then in the Publish, if IsMarkedForDeletion is true, mark IsPublished to false and delete the Id...

            return artifactResults;
        }

        #endregion Delete methods

        #region GetNavigation methods

        public List<ArtifactReference> GetNavigation(
            IUser user,
            List<IArtifact> artifacts,
            List<HttpStatusCode> expectedStatusCodes = null
            )
        {
            return GetNavigation(Address, user, artifacts, expectedStatusCodes);
        }

        /// <summary>
        /// Get ArtifactReference list which is used to represent breadcrumb navigation.
        /// (Runs:  svc/shared/navigation/{id1}/{id2}...)
        /// </summary>
        /// <param name="address">The base url of the API</param>
        /// <param name="user">The user credentials for breadcrumb navigation</param>
        /// <param name="artifacts">The list of artifacts used for breadcrumb navigation</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="readOnly">(optional) Indicator which determines if returning artifact references are readOnly or not.
        /// By default, readOnly is set to false</param>
        /// <returns>The List of ArtifactReferences after the get navigation call</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<ArtifactReference> GetNavigation(
            string address,
            IUser user,
            List<IArtifact> artifacts,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool readOnly = false
            )
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));

            string tokenValue = user.Token?.AccessControlToken;

            //Get list of artifacts which were created.
            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            var path = I18NHelper.FormatInvariant(RestPaths.Svc.Shared.NAVIGATION_ids_, string.Join("/", artifactIds));

            var queryParameters = new Dictionary<string, string>();

            if (readOnly)
            {
                queryParameters.Add("readOnly", "true");
            }
            else
            {
                queryParameters.Add("readOnly", "false");
            }

            var restApi = new RestApiFacade(address, tokenValue);

            var response = restApi.SendRequestAndDeserializeObject<List<ArtifactReference>>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes);

            return response;
        }

        #endregion GetNavigation methods

        #region Publish methods

        public virtual void Publish(IUser user = null,
            bool shouldKeepLock = false,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform Publish.");
                user = CreatedBy;
            }

            var artifactToPublish = new List<IArtifactBase> { this };

            PublishArtifacts(artifactToPublish, Address, user, shouldKeepLock, expectedStatusCodes, sendAuthorizationAsCookie);
        }

        /// <summary>
        /// Publish Artifact(s) (Used when publishing a single artifact OR a list of artifacts)
        /// </summary>
        /// <param name="artifactsToPublish">The list of artifacts to publish</param>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="shouldKeepLock">(optional) Boolean parameter which defines whether or not to keep the lock after publishing the artfacts</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The list of PublishArtifactResult objects created by the publish artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<PublishArtifactResult> PublishArtifacts(List<IArtifactBase> artifactsToPublish,
            string address,
            IUser user,
            bool shouldKeepLock = false,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactsToPublish, nameof(artifactsToPublish));

            string tokenValue = user.Token?.OpenApiToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            var additionalHeaders = new Dictionary<string, string>();

            if (shouldKeepLock)
            {
                additionalHeaders.Add("KeepLock", "true");
            }

            RestApiFacade restApi = new RestApiFacade(address, tokenValue);

            var publishedResultList = restApi.SendRequestAndDeserializeObject<List<PublishArtifactResult>, List<IArtifactBase>>(
                RestPaths.OpenApi.VersionControl.PUBLISH,
                RestRequestMethod.POST,
                artifactsToPublish,
                additionalHeaders: additionalHeaders,
                expectedStatusCodes: expectedStatusCodes);

            var deletedArtifactsList = new List<IArtifactBase>();

            // When each artifact is successfully published, set IsSaved flag to false since there are no longer saved changes
            foreach (var publishedResult in publishedResultList)
            {
                Logger.WriteDebug("Result Code for the Published Artifact {0}: {1}", publishedResult.ArtifactId, publishedResult.ResultCode);

                var publishedArtifact = artifactsToPublish.Find(a => a.Id.Equals(publishedResult.ArtifactId));

                if (publishedResult.ResultCode == HttpStatusCode.OK)
                {
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
                deletedArtifactsList[0]?.NotifyArtifactDeletion(deletedArtifactsList);
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

        /// <seealso cref="NotifyArtifactDeletion(List{IArtifactBase})"/>
        public void NotifyArtifactDeletion(List<IArtifactBase> deletedArtifactsList)
        {
            ThrowIf.ArgumentNull(deletedArtifactsList, nameof(deletedArtifactsList));

            // Notify the observers about any artifacts that were deleted as a result of this publish.
            foreach (var deletedArtifact in deletedArtifactsList)
            {
                IEnumerable<int> deletedArtifactIds =
                    from result in ((ArtifactBase)deletedArtifact).DeletedArtifactResults
                    select result.ArtifactId;

                Logger.WriteDebug("*** Notifying observers about deletion of artifact IDs: {0}", string.Join(", ", deletedArtifactIds));
                deletedArtifact.ArtifactObservers?.ForEach(o => o.NotifyArtifactDeletion(deletedArtifactIds));
            }
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
                "Address"
            };

            foreach (PropertyInfo sourcePropertyInfo in sourceArtifactBase.GetType().GetProperties())
            {
                PropertyInfo destinationPropertyInfo = destinationArtifactBase.GetType().GetProperties().First(p => p.Name == sourcePropertyInfo.Name);

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
        public void ReplacePropertiesWithPropertiesFromSourceArtifactDetails(ArtifactDetails sourceArtifact, IProject project, IUser user)
        {
            ThrowIf.ArgumentNull(sourceArtifact, nameof(sourceArtifact));

            Id = sourceArtifact.Id;
            Name = sourceArtifact.Name;
            ParentId = sourceArtifact.ParentId;
            ProjectId = sourceArtifact.ProjectId;
            Version = sourceArtifact.Version;

            // Now set the list of Property objects.
            AddOrReplaceTextOrChoiceValueProperty("Name", sourceArtifact.Name, project, user);
            AddOrReplaceTextOrChoiceValueProperty("Description", sourceArtifact.Description, project, user);

            // TODO: There is also an "ID" property to set which has a value of <prefix> + <Id>.  Ex. "AC1503".

            if (sourceArtifact.CreatedOn != null)
            {
                AddOrReplaceDateValueProperty("Created On", sourceArtifact.CreatedOn.Value, project, user);
            }

            if (sourceArtifact.LastEditedOn != null)
            {
                AddOrReplaceDateValueProperty("Last Edited On", sourceArtifact.LastEditedOn.Value, project, user);
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
        private void AddOrReplaceTextOrChoiceValueProperty(string propertyName, string propertyValue, IProject project, IUser user)
        {
            OpenApiProperty property = Properties.Find(p => p.Name == propertyName);

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
        private void AddOrReplaceDateValueProperty(string propertyName, DateTime propertyValue, IProject project, IUser user)
        {
            OpenApiProperty property = Properties.Find(p => p.Name == propertyName);

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
                OpenApiProperty property = Properties.Find(p => p.Name == propertyName);

                UsersAndGroups userOrGroup = new UsersAndGroups
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
            if (artifactList == null)
            {
                return;
            }

            var savedArtifactsDictionary = new Dictionary<IUser, List<IArtifactBase>>();

            // Separate the published from the unpublished artifacts.  Delete the published ones, and discard the saved ones.
            foreach (var artifact in artifactList.ToArray())
            {
                if (artifact.IsPublished)
                {
                    if (artifact.IsMarkedForDeletion)
                    {
                        artifact.Publish(artifact.LockOwner);
                    }
                    else
                    {
                        artifact.Delete(artifact.LockOwner, deleteChildren: true);
                        artifact.Publish(artifact.LockOwner);
                    }
                }
                else if (artifact.IsSaved)
                {
                    if ((artifact.LockOwner != null) && savedArtifactsDictionary.ContainsKey(artifact.LockOwner))
                    {
                        savedArtifactsDictionary[artifact.LockOwner].Add(artifact);
                    }
                    else if (savedArtifactsDictionary.ContainsKey(artifact.CreatedBy))
                    {
                        savedArtifactsDictionary[artifact.CreatedBy].Add(artifact);
                    }
                    else
                    {
                        savedArtifactsDictionary.Add(artifact.CreatedBy, new List<IArtifactBase> { artifact });
                    }
                }

                artifact.UnregisterObserver(observer);
            }

            // For each user that created artifacts, discard the list of artifacts they created.
            foreach (IUser user in savedArtifactsDictionary.Keys)
            {
                Logger.WriteDebug("*** Discarding all unpublished artifacts created by user: '{0}'.", user.Username);
                Artifact.DiscardArtifacts(savedArtifactsDictionary[user], savedArtifactsDictionary[user].First().Address, user);
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