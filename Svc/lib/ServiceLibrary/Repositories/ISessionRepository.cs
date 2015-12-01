using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
	public interface ISessionRepository
	{
		Task<Dictionary<string, Dictionary<string, string>>> GetConfig();
	}
}
