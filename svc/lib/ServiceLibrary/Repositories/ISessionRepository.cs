using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
	public interface ISessionRepository
	{
		Task GetAccessAsync(HttpRequestMessage request, string op, int aid);
	}
}
