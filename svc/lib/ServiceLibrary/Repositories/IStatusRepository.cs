using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
	public interface IStatusRepository
	{
        string Name { get; set; }
		Task<string> GetStatus(int timeout);
	}
}
