using FluentMigrator.Builders.Create.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageUtils
{
    public enum IdentityType
    {
        Int,
        Guid
    }

    public static class MigrationExtensions
    {

        public static ICreateTableColumnOptionOrWithColumnSyntax WithIdColumn(this ICreateTableWithColumnSyntax tableWithColumnSyntax, string name, IdentityType type)
        {
            ICreateTableColumnOptionOrWithColumnSyntax col = null;

            switch (type)
            {
                case IdentityType.Int:
                    col = tableWithColumnSyntax
                        .WithColumn(name)
                        .AsInt32()
                        .NotNullable()
                        .PrimaryKey()
                        .Identity();
                    break;
                case IdentityType.Guid:
                    col = tableWithColumnSyntax
                        .WithColumn(name)
                        .AsGuid()
                        .NotNullable()
                        .WithDefaultValue("newsequentialid()")
                        .PrimaryKey();
                    break;
            }

            return col;
        }
    }
}