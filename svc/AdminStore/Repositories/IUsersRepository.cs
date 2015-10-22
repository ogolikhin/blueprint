using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;

namespace AdminStore.Repositories
{
	public interface IUsersRepository
	{
		Task<User> GetUser(Guid guid);
		Task<Guid?[]> BeginUser(int id);
		Task EndUser(Guid guid);
		Task<IEnumerable<User>> SelectUsers(int ps, int pn);
	}
}
