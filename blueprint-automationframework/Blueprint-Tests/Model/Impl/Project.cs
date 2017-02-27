using System;
using System.Runtime.Serialization;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using Common;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.OpenApiModel.Services;
using NUnit.Framework;
using Utilities;

namespace Model.Impl
{
    [DataContract(Name = "Project", Namespace = "Model")]
    public class Project : IProject
    {
        #region Properties

        #region Serialized JSON Properties

        /// <summary>
        /// Id of the project
        /// </summary>
        [JsonProperty("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Name of the project
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        /// <summary>
        /// Description of the project
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        #endregion Serialized JSON Properties

        /// <summary>
        /// Full path for the project. e.g. /Blueprint/Project
        /// </summary>
        public string Location { get; set; }

        [JsonConverter(typeof(SerializationUtilities.ConcreteConverter<List<OpenApiArtifactType>>))]
        public List<OpenApiArtifactType> ArtifactTypes { get; } = new List<OpenApiArtifactType>();

        [JsonIgnore]
        public List<NovaArtifactType> NovaArtifactTypes { get; } = new List<NovaArtifactType>();

        [JsonIgnore]
        public List<NovaPropertyType> NovaPropertyTypes { get; } = new List<NovaPropertyType>();

        [JsonIgnore]
        public List<NovaArtifactType> NovaSubArtifactTypes { get; } = new List<NovaArtifactType>();

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Creates a new project on the Blueprint server.
        /// </summary>
        public void CreateProject()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes a project on the Blueprint server.
        /// </summary>
        public void DeleteProject()
        {
            throw new NotImplementedException();
        }

        /// <seealso cref="IProject.GetDefaultCollectionOrBaselineReviewFolder(string, IUser, BaselineAndCollectionTypePredefined)"/>
        public INovaArtifact GetDefaultCollectionOrBaselineReviewFolder(string address, IUser user,
            BaselineAndCollectionTypePredefined folderType)
        {
            ThrowIf.ArgumentNull(folderType, nameof(folderType));
            var expectedTypesPredefined = new List<BaselineAndCollectionTypePredefined> { BaselineAndCollectionTypePredefined.BaselineFolder,
            BaselineAndCollectionTypePredefined.CollectionFolder};
            Assert.IsTrue(expectedTypesPredefined.Contains(folderType), "Method works for BaselineFolder or CollectionFolder only.");

            var novaArtifacts = ArtifactStore.GetProjectChildrenByProjectId(address, Id, user);

            Assert.That((novaArtifacts != null) && novaArtifacts.Any(),
                "No artifacts were found in Project ID: {0}.", Id);

            return novaArtifacts.Find(a => a.PredefinedType.Value == (int)folderType);
        }

        /// <seealso cref="IProject.GetItemTypeIdForPredefinedType(ItemTypePredefined)"/>
        public int GetItemTypeIdForPredefinedType(ItemTypePredefined predefinedType)
        {
            var itemType = NovaArtifactTypes.Find(at => at.PredefinedType == predefinedType);
            Assert.NotNull(itemType, "No Nova artifact type was found in project {0} for predefined type: '{1}'", Id, predefinedType);
            return itemType.Id;
        }

        /// <seealso cref="IProject.GetProjects(string, IUser)"/>
        public List<IProject> GetProjects(string address, IUser user = null)
        {
            return OpenApi.GetProjects(address, user);
        }

        /// <seealso cref="IProject.GetProject(string, int, IUser)"/>
        public IProject GetProject(string address, int projectId, IUser user = null)
        {
            return OpenApi.GetProject(address, projectId, user);
        }

        /// <summary>
        /// Updates a project on the Blueprint server with the changes that were made to this object.
        /// </summary>
        public void UpdateProject()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        /// <returns>A string representation of this object.</returns>
        public override string ToString()
        {
            return I18NHelper.FormatInvariant("[Project]: Id={0}, Name={1}, Description={2}, Location={3}", Id, Name, Description, Location);
        }

        /// <seealso cref="IProject.GetAllOpenApiArtifactTypes(string, IUser, bool, List{HttpStatusCode})"/>
        public List<OpenApiArtifactType> GetAllOpenApiArtifactTypes(
            string address,
            IUser user,
            bool shouldRetrievePropertyTypes = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var artifactTypes = OpenApi.GetAllArtifactTypes(address, Id, user, shouldRetrievePropertyTypes,
                expectedStatusCodes);

            // Clean and repopulate ArtifactTypes if there is any element exist for ArtifactTypes
            if (ArtifactTypes.Any())
            {
                ArtifactTypes.Clear();
            }

            foreach (var artifactType in artifactTypes)
            {
                ArtifactTypes.Add(artifactType);
            }

            return artifactTypes;
        }

        /// <seealso cref="IProject.GetAllNovaArtifactTypes(IArtifactStore, IUser, List{HttpStatusCode})"/>
        public List<NovaArtifactType> GetAllNovaArtifactTypes(
            IArtifactStore artifactStore,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));

            var artifactTypesResult = artifactStore.GetCustomArtifactTypes(this, user, expectedStatusCodes);
            var artifactTypes = artifactTypesResult.ArtifactTypes;

            // Clean and repopulate NovaArtifactTypes if there is any element exist for NovaArtifactTypes.
            if (NovaArtifactTypes.Any())
            {
                NovaArtifactTypes.Clear();
            }

            NovaArtifactTypes.AddRange(artifactTypes);

            if (NovaPropertyTypes.Any())
            {
                NovaPropertyTypes.Clear();
            }

            NovaPropertyTypes.AddRange(artifactTypesResult.PropertyTypes);

            if (NovaSubArtifactTypes.Any())
            {
                NovaSubArtifactTypes.Clear();
            }

            NovaSubArtifactTypes.AddRange(artifactTypesResult.SubArtifactTypes);

            return artifactTypes;
        }

        /// <seealso cref="IProject.GetNovaBaseItemTypeId(ItemTypePredefined)"/>
        public int GetNovaBaseItemTypeId(ItemTypePredefined itemTypePredefined)
        {
            ThrowIf.ArgumentNull(itemTypePredefined, nameof(itemTypePredefined));
            Assert.IsNotEmpty(NovaArtifactTypes,
                "Call GetAllNovaArtifactTypes to get Nova Artifact type before using this method.");
            var novaArtifactType = NovaArtifactTypes.Find(t => (int)(t.PredefinedType) == (int)itemTypePredefined);
            Assert.IsNotNull(novaArtifactType, "NovaItemType was not found for the specefied ItemTypePredefined.");
            return novaArtifactType.Id;
        }

        #endregion Public Methods

    }
}
