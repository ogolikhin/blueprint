using System.Collections.Generic;
using System.Data;
using ArtifactStore.Helpers.Validators;
using ArtifactStore.Models.PropertyTypes;
using ArtifactStore.Repositories;
using BluePrintSys.Messaging.CrossCutting.Models;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Reuse;

namespace ArtifactStore.Models.Workflow
{
    

    public class ExecutionParameters : IExecutionParameters
    {
        public ItemTypeReuseTemplate ReuseItemTemplate { get; private set; }

        public List<DPropertyType> CustomPropertyTypes { get; private set; }
        public IDbTransaction Transaction { get; private set; }

        public IReadOnlyList<IPropertyValidator> Validators { get; private set; }
        public IReusePropertyValidator ReuseValidator { get; private set; }

        public ISaveArtifactRepository SaveRepository { get; private set; }
        public VersionControlArtifactInfo ArtifactInfo { get; private set; }
        public int UserId { get; private set; }

        public ExecutionParameters(
            int userId,
            VersionControlArtifactInfo artifactInfo,
            ItemTypeReuseTemplate reuseTemplate,
            List<DPropertyType> customPropertyTypes,
            ISaveArtifactRepository saveArtifactRepository,
            IDbTransaction transaction,
            IReadOnlyList<IPropertyValidator> validators,
            IReusePropertyValidator reuseValidator
            )
        {
            UserId = userId;
            ArtifactInfo = artifactInfo;
            ReuseItemTemplate = reuseTemplate;
            CustomPropertyTypes = customPropertyTypes;
            SaveRepository = saveArtifactRepository;
            Transaction = transaction;
            Validators = validators;
            ReuseValidator = reuseValidator;
        }
        public ExecutionParameters(
            int userId,
            VersionControlArtifactInfo artifactInfo,
            ItemTypeReuseTemplate reuseTemplate,
            List<DPropertyType> customPropertyTypes,
            ISaveArtifactRepository saveArtifactRepository,
            IDbTransaction transaction): this(
                userId, 
                artifactInfo, 
                reuseTemplate, 
                customPropertyTypes, 
                saveArtifactRepository, 
                transaction, 
                new List<IPropertyValidator>()
                {
                    new NumberPropertyValidator()

                }, 
                new ReusePropertyValidator())
        {
        }
    }
}