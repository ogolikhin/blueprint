using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Workflow;

namespace ServiceLibrary.Models.VersionControl
{
    public interface ISaveArtifactRepository
    {
        Task SavePropertyChangeActions(
            int userId,
            IEnumerable<IPropertyChangeAction> actions,
            IEnumerable<WorkflowPropertyType> propertyTypes,
            VersionControlArtifactInfo artifact,
            IDbTransaction transaction = null);
        Task UpdateArtifactName(
            int userId,
            int artifactId,
            string artifactName,
            IDbTransaction transaction = null);
    }
}
