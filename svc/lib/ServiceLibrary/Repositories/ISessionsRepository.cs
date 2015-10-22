using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
	public interface ISessionsRepository
	{
		Task<Session> GetSession(Guid guid);
		Task<IEnumerable<Session>> SelectSessions(int ps, int pn);
		Task<Guid?[]> BeginSession(int id);
		Task EndSession(Guid guid);
	}
}
