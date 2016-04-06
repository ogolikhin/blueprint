using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Helpers;

namespace Logging.Database.TestObjects
{
    public abstract class LocalDatabaseContext : ContextBase
    {
        protected const string LocalDbConnectionString = @"Data Source=(LocalDB)\v11.0;Initial Catalog=master;Integrated Security=True";

        protected string DbFileName;
        protected string DbLogFileName;

        protected string DbName;
        protected SqlConnection LocalDbConnection = new SqlConnection(LocalDbConnectionString);

        protected abstract string GetLocalDatabaseFileName();

        protected override void Given()
        {
            this.DbName = this.GetLocalDatabaseFileName();

            if (string.IsNullOrWhiteSpace(DbName))
            {
                Assert.Inconclusive("You must specify a valid database name");
            }

            var output = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            this.DbFileName = Path.Combine(output, DbName + ".mdf");
            this.DbLogFileName = Path.Combine(output, I18NHelper.FormatInvariant("{0}_log.ldf", DbName));

            this.LocalDbConnection.Open();

            // Recover from failed run
            this.DetachDatabase();

            File.Delete(this.DbFileName);
            File.Delete(this.DbLogFileName);

            using (var cmd = new SqlCommand(I18NHelper.FormatInvariant("CREATE DATABASE {0} ON (NAME = N'{0}', FILENAME = '{1}')", this.DbName, this.DbFileName), this.LocalDbConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        protected override void OnCleanup()
        {
            var sql = I18NHelper.FormatInvariant("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE", DbName);
            using (var cmd = new SqlCommand(sql, this.LocalDbConnection))
            {
                cmd.ExecuteNonQuery();
            }

            this.LocalDbConnection.ChangeDatabase("master");
            this.DetachDatabase();
            this.LocalDbConnection.Dispose();

            File.Delete(this.DbFileName);
            File.Delete(this.DbLogFileName);
        }

        protected string GetSqlConnectionString()
        {
            return I18NHelper.FormatInvariant(@"Data Source=(LocalDB)\v11.0;AttachDBFileName={1};Initial Catalog={0};Integrated Security=True;", DbName, DbFileName);
        }

        protected void DetachDatabase()
        {
            using (var cmd = new SqlCommand(I18NHelper.FormatInvariant("IF EXISTS (SELECT * FROM sys.databases WHERE Name = N'{0}') exec sp_detach_db N'{0}'", DbName), LocalDbConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
