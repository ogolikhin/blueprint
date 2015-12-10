using ServiceLibrary.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccessControl.Repositories
{
    public interface ISessionsRepository
    {
        Task<Session> GetSession(Guid guid);
        Task<Session> GetUserSession(int uid);
        Task<IEnumerable<Session>> SelectSessions(int ps, int pn);
        Task<Guid?[]> BeginSession(int userId, string userName, int licenseLevel, bool isSso);
        Task EndSession(Guid guid);

	    Task<int> GetActiveLicenses(int excludeUserId, int licenseLevel, int licenseLockTimeMinutes);
    }
}
