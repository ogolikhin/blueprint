using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccessControl.Models;

namespace AccessControl.Repositories
{
	public interface ISessionsRepository
	{
		Task<Guid?> CreateSession(Session session);
		Task<Guid?> UpdateSession(Session session);
		Task<Session> ReadSession(Guid guid);
		Task<Guid?> DeleteSession(Guid guid);
		Task<IEnumerable<Session>> SelectSessions(int ps, int pn);
	}
}
