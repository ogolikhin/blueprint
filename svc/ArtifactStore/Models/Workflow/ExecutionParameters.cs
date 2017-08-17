using System.Collections.Generic;
using System.Data;
using ArtifactStore.Repositories;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Reuse;

namespace ArtifactStore.Models.Workflow
{
    public class ExecutionParameters
    {
        public ItemTypeReuseTemplate ReuseItemTemplate { get; private set; }

        public List<DPropertyType> CustomPropertyTypes { get; private set; }
        public IDbTransaction Transaction { get; private set; }

        public IReadOnlyList<IPropertyValidator> Validators { get; private set; }

        public ISaveArtifactRepository SaveRepository { get; private set; }
        public VersionControlArtifactInfo ArtifactInfo { get; private set; }

        public ExecutionParameters(
            VersionControlArtifactInfo artifactInfo,
            ItemTypeReuseTemplate reuseTemplate,
            List<DPropertyType> customPropertyTypes,
            ISaveArtifactRepository saveArtifactRepository,
            IDbTransaction transaction)
        {
            ArtifactInfo = artifactInfo;
            ReuseItemTemplate = reuseTemplate;
            CustomPropertyTypes = customPropertyTypes;
            SaveRepository = saveArtifactRepository;
            Transaction = transaction;
            Validators  = new List<IPropertyValidator>
            {
                new NumberPropertyValidator()
            }; 
        }
    }
}