using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
	public interface IStatusRepository
	{
        string Name { get; set; }
        string AccessInfo { get; set; }
		Task<string> GetStatus(int timeout);
	}
}
