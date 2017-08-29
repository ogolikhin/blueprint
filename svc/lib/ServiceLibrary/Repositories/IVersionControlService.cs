using System.Data;
using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Models.VersionControl;

namespace ServiceLibrary.Repositories
{
    public interface IVersionControlService
    {
        Task<ArtifactResultSet> PublishArtifacts(PublishParameters parameters, IDbTransaction transaction = null);
    }
}
