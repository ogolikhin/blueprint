using FluentMigrator;
using System;
using StorageUtils;

namespace FileStorage.Migrations.Dylan
{
    [Tags("DBSetup")]
    [Migration(1, "Create the initial version")]
    public class M00001_CreateInitialVersion : Migration
    {
        public override void Up()
        {
            Create.Table("Files")
                .WithIdColumn("FileId", IdentityType.Guid)
                .WithColumn("StoredTime").AsDateTime().NotNullable()
                .WithColumn("FileName").AsString(256).NotNullable()
                .WithColumn("FileType").AsString(64).NotNullable()
                .WithColumn("FileContent").AsBinary(Int32.MaxValue).Nullable()
                .WithColumn("FileSize").AsInt64().NotNullable();

            Execute.EmbeddedScript("DeleteFile_M00001.sql");
            Execute.EmbeddedScript("GetFile_M00001.sql");
            Execute.EmbeddedScript("GetStatus_M00001.sql");
            Execute.EmbeddedScript("HeadFile_M00001.sql");
            Execute.EmbeddedScript("PostFile_M00001.sql");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

    }
}
