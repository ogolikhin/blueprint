using System.Collections.Generic;
using System.Data;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Reuse;
using ServiceLibrary.Models.VersionControl;

namespace ServiceLibrary.Models.Workflow
{
    public class ExecutionParameters : IExecutionParameters
    {
        public ItemTypeReuseTemplate ReuseItemTemplate { get; }

        public List<WorkflowPropertyType> CustomPropertyTypes { get; }

        public IDbTransaction Transaction { get; }

        public IReadOnlyList<IPropertyValidator> Validators { get; }

        public IReusePropertyValidator ReuseValidator { get; }

        public ISaveArtifactRepository SaveRepository { get; }

        public VersionControlArtifactInfo ArtifactInfo { get; }

        public int UserId { get; }

        public IValidationContext ValidationContext { get; }

        public ExecutionParameters(
            int userId,
            VersionControlArtifactInfo artifactInfo,
            ItemTypeReuseTemplate reuseTemplate,
            List<WorkflowPropertyType> customPropertyTypes,
            ISaveArtifactRepository saveArtifactRepository,
            IDbTransaction transaction,
            IValidationContext validationContext,
            IReadOnlyList<IPropertyValidator> validators,
            IReusePropertyValidator reuseValidator)
        {
            UserId = userId;
            ArtifactInfo = artifactInfo;
            ReuseItemTemplate = reuseTemplate;
            CustomPropertyTypes = customPropertyTypes;
            SaveRepository = saveArtifactRepository;
            Transaction = transaction;
            Validators = validators;
            ReuseValidator = reuseValidator;
            ValidationContext = validationContext;
        }
        public ExecutionParameters(
            int userId,
            VersionControlArtifactInfo artifactInfo,
            ItemTypeReuseTemplate reuseTemplate,
            List<WorkflowPropertyType> customPropertyTypes,
            ISaveArtifactRepository saveArtifactRepository,
            IDbTransaction transaction,
            IValidationContext validationContext) : this(
                userId, 
                artifactInfo, 
                reuseTemplate, 
                customPropertyTypes, 
                saveArtifactRepository, 
                transaction,
                validationContext,
                new List<IPropertyValidator>()
                {
                    new TextPropertyValidator(),
                    new NumberPropertyValidator(),
                    new DatePropertyValidator(),
                    new UserPropertyValidator()
                }, 
                new ReusePropertyValidator())
        {
        }
    }
}