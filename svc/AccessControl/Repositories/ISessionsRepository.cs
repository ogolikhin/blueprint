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
        Task<Session> BeginSession(int userId, string userName, int licenseLevel, bool isSso, Action<Guid> oldSessionIdAction = null);
        Task<Session> ExtendSession(Guid guid);
        Task<Session> EndSession(Guid guid, bool timeout);
    }
}
