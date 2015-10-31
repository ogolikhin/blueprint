using FluentMigrator;
using System;
using StorageUtils;

namespace FileStorage.Migrations.Iteration1
{
    [Migration(1, "Create the initial version")]
    public class M00001_CreateInitialVersion : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("EnableFullText.sql");

            Create.Table("Files")
                .WithIdColumn("FileId", IdentityType.Guid)
                .WithColumn("StoredTime").AsDateTime().NotNullable()
                .WithColumn("FileName").AsString(256).NotNullable()
                .WithColumn("FileType").AsString(64).NotNullable()
                .WithColumn("FileContent").AsBinary(Int32.MaxValue).Nullable()
                .WithColumn("FileSize").AsInt64().NotNullable();

            Execute.EmbeddedScript("DeleteFile.sql");
            Execute.EmbeddedScript("GetFile.sql");
            Execute.EmbeddedScript("GetStatus.sql");
            Execute.EmbeddedScript("HeadFile.sql");
            Execute.EmbeddedScript("PostFile.sql");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

    }
}
