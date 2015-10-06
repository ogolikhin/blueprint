using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AccessControl.Models;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading;
using System.Web.Http.Results;

namespace AccessControl.Repositories
{
	public class CacheSqlSessionsRepository : SqlSessionsRepository
	{
		private static readonly MemoryCache Cache = new MemoryCache("SessionsCache");
		public override async Task<Guid?> CreateSession(int ext)
		{
		}

		public override async Task<Session> ReadSession(Guid guid, int ext)
		{
			var key = Session.Convert(guid);
			var session = Cache.Get(key) as Session;
			if (session != null)
			{
				return session;
			}
			lock (Cache)
			{
				if (Cache.Get(key) == null)
				{
					Cache.Add(key, base.ReadSession(guid, ext), new CacheItemPolicy()
					{
						SlidingExpiration = TimeSpan.FromSeconds(ext),
						UpdateCallback = new CacheEntryUpdateCallback(a => base.DeleteSession(Session.Convert(a.Key)))
					});
				}
			}
			var val = Cache.Get(key);
			if (val is Session)
			{
				return val as Session;
			}
			session = await (val as Task<Session>);
			if (session == null)
			{
				throw new KeyNotFoundException();
			}
			Cache[key] = session;
			return session;
		}

		public override async Task<Guid?> DeleteSession(Guid guid)
		{
			Session session = Cache.Remove(Session.Convert(guid)) as Session;
			if (session == null)
			{
				return await base.DeleteSession(guid);
			}
			return session.SessionId;
		}

		public override async Task<IEnumerable<Session>> SelectSessions(int ps, int pn)
		{
		}
	}
}