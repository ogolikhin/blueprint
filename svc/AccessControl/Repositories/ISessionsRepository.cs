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
		Task<Guid?> PostFile(Session session);
		Task<Session> HeadFile(Guid guid);
		Task<Session> GetFile(Guid guid);
		Task<Guid?> DeleteFile(Guid guid);
		Task<bool> GetStatus();
	}
}
