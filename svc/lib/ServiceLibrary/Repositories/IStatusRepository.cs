using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
	public interface IStatusRepository
	{
		Task<bool> GetStatus();
	}
}
