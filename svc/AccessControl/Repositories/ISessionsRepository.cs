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
		Task<Guid?> BeginSession(int id);
		Task EndSession(Guid guid);
		Task<IEnumerable<Session>> SelectSessions(int ps, int pn);
	}
}
