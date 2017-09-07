using System.Collections.Generic;
using System.Data;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Reuse;
using ServiceLibrary.Models.VersionControl;

namespace ServiceLibrary.Models.Workflow
{
    public interface IExecutionParameters
    {
        ItemTypeReuseTemplate ReuseItemTemplate { get; }
        List<WorkflowPropertyType> CustomPropertyTypes { get; }
        IDbTransaction Transaction { get; }
        IReadOnlyList<IPropertyValidator> Validators { get; }
        IReusePropertyValidator ReuseValidator { get; }
        IValidationContext ValidationContext { get; }
        int UserId { get; }

        ISaveArtifactRepository SaveRepository { get; }

        VersionControlArtifactInfo ArtifactInfo { get; }

    }
}
