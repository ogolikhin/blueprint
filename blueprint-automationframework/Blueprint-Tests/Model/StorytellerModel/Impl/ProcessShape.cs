using System;
using System.Collections.Generic;
using System.Globalization;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.StorytellerModel.Enums;
using Newtonsoft.Json;
using Utilities;

namespace Model.StorytellerModel.Impl
{
    public class ProcessShape: IProcessShape
    {
        private const string StorytellerProcessPrefix = "SP";

        public int Id { get; set; }

        public string Name { get; set; }

        public int ParentId { get; set; }

        public int ProjectId { get; set; }

        public string TypePrefix { get; set; }

        public ItemTypePredefined BaseItemTypePredefined { get; set; }

        [JsonConverter(typeof(SerializationUtilities.ConcreteDictionaryConverter<Dictionary<string, PropertyValueInformation>, PropertyValueInformation>))]
        public Dictionary<string, PropertyValueInformation> PropertyValues { get; set; }

        public ArtifactReference AssociatedArtifact { get; set; }

        public ArtifactReference PersonaReference { get; set; }

        public ProcessShape()
        {
            PropertyValues = new Dictionary<string, PropertyValueInformation>();
        }

        /// <seealso cref="IProcessShape.AddAssociatedArtifact(INovaArtifactDetails)"/>
        public ArtifactReference AddAssociatedArtifact(INovaArtifactDetails artifact)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            if (artifact.PredefinedType != null && artifact.ProjectId != null)
            {
                AssociatedArtifact = new ArtifactReference()
                {
                    Id = artifact.Id,
                    Link = null,
                    Name = artifact.Name,
                    ProjectId = artifact.ProjectId.Value,
                    TypePrefix = artifact.Prefix,
                    BaseItemTypePredefined = (ItemTypePredefined)artifact.PredefinedType.Value,
                    Version = artifact.Version
                };
            }

            return AssociatedArtifact;
        }

        /// <seealso cref="IProcessShape.AddPersonaReference(INovaArtifactDetails)"/>
        public ArtifactReference AddPersonaReference(INovaArtifactDetails artifact = null)
        {
            if (artifact?.PredefinedType != null && artifact.ProjectId != null)
            {
                PersonaReference = new ArtifactReference()
                {
                    Id = artifact.Id,
                    Link = null,
                    Name = artifact.Name,
                    ProjectId = artifact.ProjectId.Value,
                    TypePrefix = artifact.Prefix,
                    BaseItemTypePredefined = (ItemTypePredefined)artifact.PredefinedType.Value,
                    Version = artifact.Version
                };
            }

            return PersonaReference;
        }

        /// <seealso cref="IProcessShape.AddDefaultPersonaReference(ProcessShapeType)"/>
        public ArtifactReference AddDefaultPersonaReference(ProcessShapeType processShapeType)
        {
            if (processShapeType == ProcessShapeType.UserTask || processShapeType == ProcessShapeType.SystemTask ||
                processShapeType == ProcessShapeType.PreconditionSystemTask)
            {
                PersonaReference = new ArtifactReference()
                {
                    Id = processShapeType == ProcessShapeType.UserTask ? -1 : -2,
                    Link = null,
                    Name = processShapeType == ProcessShapeType.UserTask ? "User" : "System",
                    ProjectId = 0,
                    TypePrefix = null,
                    BaseItemTypePredefined = ItemTypePredefined.Actor,
                    Version = null
                };
            }
            return PersonaReference;
        }

        /// <seealso cref="IProcessShape.GetShapeType()"/>
        public ProcessShapeType GetShapeType()
        {
            string clientType = PropertyTypeName.ClientType.ToString();

            clientType = PropertyValues.ContainsKey(clientType) ? clientType : clientType.LowerCaseFirstCharacter();

            return (ProcessShapeType) Convert.ToInt32(PropertyValues[clientType].Value, CultureInfo.InvariantCulture);
        }

        /// <seealso cref="IProcessShape.IsTypeOf(ProcessShapeType)"/>
        public bool IsTypeOf(ProcessShapeType processShapeType)
        {
            return GetShapeType() == processShapeType;
        }
    }
}