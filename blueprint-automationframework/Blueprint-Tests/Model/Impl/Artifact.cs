using Common;
using System;
using System.Net;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using Model.Factories;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;
using System.Data.SqlClient;
using System.Globalization;

namespace Model.Impl
{
    public class Artifact : IArtifact
    {

        private const string SVC_PATH = "api/v1/projects";
        private const string URL_ARTIFACTS = "artifacts";
        private const string URL_PUBLISH = "api/v1/vc/publish";
        private const string URL_DISCARD = "api/v1/vc/discard";
        private const string URL_COMMENTS = "comments";
        private const string URL_REPLIES = "replies";
        private string _address = null;

        #region Properties
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public int Version { get; set; }
        public int ParentId { get; set; }
        public Uri BlueprintUrl { get; set; }
        public int ArtifactTypeId { get; set; }
        #endregion Properties

        /// <summary>
        /// Constructor
        /// </summary>
        public Artifact()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address">The URI address of the Artifact REST API</param>
        public Artifact(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));
            _address = address;
        }

        /// <summary>
        /// Adds the specified artifact to ArtifactStore.
        /// </summary>
        /// <param name="artifact">The artifact to add.</param>
        /// <param name="user">The user to authenticate to the ArtifactStore.</param>
        /// <param name="expectedStatusCodes">A list of expected status codes.  By default, only '201' is expected.</param>
        /// <returns>The artifact that was created (including the artifact ID that ArtifactStore gave it).</returns>
        /// <exception cref="WebException">A WebException sub-class if ArtifactStore returned an unexpected HTTP status code.</exception>
        public IArtifact AddArtifact(IArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(SVC_PATH + "/{0}/" + URL_ARTIFACTS, artifact.ProjectId);

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode>();
                expectedStatusCodes.Add(HttpStatusCode.Created);
            }

            Artifact artractObject = (Artifact)artifact;
            RestApiFacade restApi = new RestApiFacade(_address, user.Username, user.Password);
            ArtifactResult artifactResult = restApi.SendRequestAndDeserializeObject<ArtifactResult, Artifact>(path, RestRequestMethod.POST, artractObject, expectedStatusCodes: expectedStatusCodes);

            Logger.WriteDebug("Result Code: {0}", artifactResult.ResultCode);
            Logger.WriteDebug(I18NHelper.FormatInvariant("POST {0} returned followings: Message: {1}, ResultCode: {2}", path, artifactResult.Message, artifactResult.ResultCode));
            Logger.WriteDebug("The Artifact Returned: {0}", artifactResult.Artifact);

            return artifact;
        }

        public IArtifactResultBase DeleteArtifact(IArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(SVC_PATH + "/{0}/" + URL_ARTIFACTS + "/{1}/", artifact.ProjectId, artifact.Id);

            RestApiFacade restApi = new RestApiFacade(_address, user.Username, user.Password);
            ArtifactResult artifactResult = restApi.SendRequestAndDeserializeObject<ArtifactResult>(path, RestRequestMethod.DELETE, expectedStatusCodes: expectedStatusCodes);

            Logger.WriteDebug("Result Code: {0}", artifactResult.ResultCode);
            Logger.WriteDebug(I18NHelper.FormatInvariant("DELETE {0} returned followings: Message: {1}, ResultCode: {2}", path, artifactResult.Message, artifactResult.ResultCode));
            Logger.WriteDebug("The Artifact Returned: {0}", artifactResult.Artifact);

            return artifactResult;
        }
    }

    public class OpenApiArtifact : IOpenApiArtifact
    {

        private const string SVC_PATH = "api/v1/projects";
        private const string URL_ARTIFACTS = "artifacts";
        private const string URL_PUBLISH = "api/v1/vc/publish";
        private const string URL_DISCARD = "api/v1/vc/discard";
        private const string URL_COMMENTS = "comments";
        private const string URL_REPLIES = "replies";
        private string _address = null;

        #region Properties
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public int Version { get; set; }
        public int ParentId { get; set; }
        public Uri BlueprintUrl { get; set; }
        public int ArtifactTypeId { get; set; }
        public string ArtifactTypeName { get; set; }
        public string BaseArtifactType { get; set; }
        public bool AreTracesReadOnly { get; set; }
        public bool AreAttachmentsReadOnly { get; set; }
        public bool AreDocumentReferencesReadOnly { get; set; }
        [JsonConverter(typeof(Deserialization.ConcreteConverter<OpenApiProperty>))]
        public List<IOpenApiProperty> Properties { get; private set; }
        [JsonConverter(typeof(Deserialization.ConcreteConverter<OpenApiComment>))]
        public List<IOpenApiComment> Comments { get; }
        [JsonConverter(typeof(Deserialization.ConcreteConverter<OpenApiTrace>))]
        public List<IOpenApiTrace> Traces { get; }
        [JsonConverter(typeof(Deserialization.ConcreteConverter<OpenApiAttachment>))]
        public List<IOpenApiAttachment> Attachments { get; }
        #endregion Properties

        /// <summary>
        /// Constructor in order to use it as generic type
        /// </summary>
        public OpenApiArtifact()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The URI address of the artifact.</param>
        public OpenApiArtifact(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            _address = address;
        }

        /// <summary>
        /// Set Properties for the artifact object
        /// </summary>
        /// <param name="properties">The property list</param>
        /// 
        public void SetProperties(List<IOpenApiProperty> properties)
        {
            if (this.Properties == null)
            {
                Properties = new List<IOpenApiProperty>();
            }
            Properties = properties;
        }
        
        /// <summary>
        /// Update an artifact object with ArtifactTypeId, ArtifactTypeName, and ProjectId based the target project
        /// </summary>
        /// <param name="project">The target project.</param>
        /// <param name="artifact">The artifact object that contains artifactType information.</param>
        /// 
        public void UpdateArtifactType(int projectId, IOpenApiArtifact artifact)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            string query = null;
            SqlDataReader reader;

            //variables
            int query_projectId;
            int query_artifactTypeId;
            string query_artifactTypeName;

            using (IDatabase database = DatabaseFactory.CreateDatabase())
            {
                query = "SELECT Project_ItemId, ItemTypeId, Name FROM dbo.TipItemTypesView WHERE Project_ItemId = @Project_ItemId and Name = @Name;";
                using (SqlCommand cmd = database.CreateSqlCommand(query))
                {
                    database.Open();
                    cmd.Parameters.Add("@Project_ItemId", SqlDbType.Int).Value = projectId;
                    cmd.Parameters.Add("@Name", SqlDbType.NChar).Value = artifact.ArtifactTypeName;
                    cmd.CommandType = CommandType.Text;
                    using (reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                        }
                        query_projectId = Int32.Parse(reader["Project_ItemId"].ToString(), CultureInfo.InvariantCulture);
                        query_artifactTypeId = Int32.Parse(reader["ItemTypeId"].ToString(), CultureInfo.InvariantCulture);
                        query_artifactTypeName = reader["Name"].ToString();
                    }
                }
            }
            artifact.ArtifactTypeName = query_artifactTypeName;
            artifact.ProjectId = query_projectId;
            artifact.ArtifactTypeId = query_artifactTypeId;
        }

        /// <summary>
        /// Adds the specified artifact to ArtifactStore.
        /// </summary>
        /// <param name="artifact">The artifact to add.</param>
        /// <param name="user">The user to authenticate to the ArtifactStore.</param>
        /// <param name="expectedStatusCodes">A list of expected status codes.  By default, only '201' is expected.</param>
        /// <returns>The artifact result after adding artifact.</returns>
        /// <exception cref="WebException">A WebException sub-class if ArtifactStore returned an unexpected HTTP status code.</exception>
        public IOpenApiArtifactResult AddArtifact(IOpenApiArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(SVC_PATH + "/{0}/" + URL_ARTIFACTS, artifact.ProjectId);

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode>();
                expectedStatusCodes.Add(HttpStatusCode.Created);
            }

            OpenApiArtifact artractObject = (OpenApiArtifact)artifact;
            RestApiFacade restApi = new RestApiFacade(_address, user.Username, user.Password);
            OpenApiArtifactResult artifactResult = restApi.SendRequestAndDeserializeObject<OpenApiArtifactResult, OpenApiArtifact>(path, RestRequestMethod.POST, artractObject, expectedStatusCodes: expectedStatusCodes);

            Logger.WriteDebug("Result Code: {0}", artifactResult.ResultCode);
            Logger.WriteDebug(I18NHelper.FormatInvariant("POST {0} returned followings: Message: {1}, ResultCode: {2}", path, artifactResult.Message, artifactResult.ResultCode));
            Logger.WriteDebug("The Artifact Returned: {0}", artifactResult.Artifact);

            return artifactResult;
        }

        /// <summary>
        /// Update an artifact object for the target project.
        /// </summary>
        /// <param name="project">The target project.</param>
        /// <param name="artifact">The artifact object that contains artifactType information.</param>
        /// <param name="propertyName">(optional) The property name that will be added for the artifact.</param>
        /// <param name="propertyValue">(optional) The property value that will be added for the artifact.</param>
        /// <returns>The updated artifact object with auto-generated name and a required assigned property</returns>
        /// 
        public IOpenApiArtifact UpdateArtifact(IProject project, IOpenApiArtifact artifact, string propertyName = null, string propertyValue = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            artifact.Name = "REST_Artifact_" + RandomGenerator.RandomAlphaNumeric(5);
            if (propertyName == null)
            {
                propertyName = "Description";
            }
            if (propertyValue == null)
            {
                propertyValue = "DescriptionValue";
            }
            UpdateArtifactType(project.Id, artifact);
            List<IOpenApiProperty> properties = new List<IOpenApiProperty>();
            IOpenApiProperty property = new OpenApiProperty();
            properties.Add(property.CreatePropertyBasedonDB(project, propertyName, propertyValue));
            artifact.SetProperties(properties);
            return artifact;
        }

    }
}
