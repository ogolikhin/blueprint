using ServiceLibrary.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
	public interface IStatusRepository
	{
        string Name { get; set; }
        string AccessInfo { get; set; }
        Task<List<StatusResponse>> GetStatuses(int timeout);

    }
}
