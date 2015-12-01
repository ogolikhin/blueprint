using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
	public interface IConfigRepository
	{
		Task<Dictionary<string, Dictionary<string, string>>> GetConfig();
	}
}
