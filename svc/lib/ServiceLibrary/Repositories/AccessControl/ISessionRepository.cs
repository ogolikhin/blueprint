using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories.ConfigControl
{
	public interface ISessionRepository
	{
		Task GetAccessAsync(HttpRequestMessage request);
	}
}
