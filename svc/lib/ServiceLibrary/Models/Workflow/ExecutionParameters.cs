using System.Collections.Generic;
using System.Data;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Reuse;
using ServiceLibrary.Models.VersionControl;

namespace ServiceLibrary.Models.Workflow
{
    public class ExecutionParameters : IExecutionParameters
    {
        public ItemTypeReuseTemplate ReuseItemTemplate { get; }

        public List<DPropertyType> CustomPropertyTypes { get; }

        public IDbTransaction Transaction { get; }

        public IReadOnlyList<IPropertyValidator> Validators { get; }

        public IReusePropertyValidator ReuseValidator { get; }

        public ISaveArtifactRepository SaveRepository { get; }

        public VersionControlArtifactInfo ArtifactInfo { get; }

        public int UserId { get; }

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
                    new NumberPropertyValidator(),
                    new DatePropertyValidator()

                }, 
                new ReusePropertyValidator())
        {
        }
    }
}