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
		Task<Guid?> CreateSession(int ext);
		Task<Session> ReadSession(Guid guid, int ext);
		Task<Guid?> DeleteSession(Guid guid);
		Task<IEnumerable<Session>> SelectSessions(int ps, int pn);
	}
}
