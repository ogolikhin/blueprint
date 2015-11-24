using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AccessControl.Models;

namespace AccessControl.Repositories
{
    public interface ISessionsRepository
    {
        Task<Session> GetSession(Guid guid);
        Task<Session> GetUserSession(int uid);
        Task<IEnumerable<Session>> SelectSessions(int ps, int pn);
        Task<Guid?[]> BeginSession(int userId, string userName, int licenseLevel, bool samlUser);
        Task EndSession(Guid guid);
    }
}
