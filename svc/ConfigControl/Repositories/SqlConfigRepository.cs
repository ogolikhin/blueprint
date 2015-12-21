﻿using System.Data;
using System.Threading.Tasks;
using Dapper;
using ConfigControl.Models;
using System.Collections.Generic;
using ServiceLibrary.Repositories;
using System.Configuration;

namespace ConfigControl.Repositories
{
    public class SqlConfigRepository : IConfigRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlConfigRepository()
            : this(new SqlConnectionWrapper(WebApiConfig.AdminStorage))
        {
        }

        internal SqlConfigRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public virtual async Task<IEnumerable<ConfigSetting>> GetSettings(bool allowRestricted)
        {
            var prm = new DynamicParameters();
            prm.Add("@AllowRestricted", allowRestricted);
            return await _connectionWrapper.QueryAsync<ConfigSetting>("GetConfigSettings", prm, commandType: CommandType.StoredProcedure);
        }

        public static string AdminStorageDatabase
        {
            get
            {
                var cn = ConfigurationManager.ConnectionStrings["AdminStorage"].ConnectionString;

                return cn;
            }
        }

    }
}
