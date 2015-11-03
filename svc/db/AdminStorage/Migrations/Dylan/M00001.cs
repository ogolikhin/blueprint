using FluentMigrator;
using System;
using StorageUtils;

namespace AdminStorage.Migrations.Dylan
{
    [Tags("DBSetup")]
    [Migration(1, "Create the initial version")]
    public class M00001 : Migration
    {
        public override void Up()
        {
            Create.Table("ApplicationLabels")
                .WithColumn("Key").AsString(64).NotNullable()
                .WithColumn("Locale").AsString(32).NotNullable()
                .WithColumn("Text").AsString(128).NotNullable();
            Create.PrimaryKey().OnTable("ApplicationLabels").Columns("Key", "Locale");

            Create.Table("ConfigSettings")
                .WithColumn("Key").AsString(64).NotNullable().PrimaryKey()
                .WithColumn("Value").AsString(128).NotNullable()
                .WithColumn("Group").AsString(128).NotNullable()
                .WithColumn("IsRestricted").AsBoolean().NotNullable();

            Create.Table("Sessions")
                .WithColumn("UserId").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("SessionId").AsGuid().NotNullable()
                .WithColumn("BeginTime").AsDateTime().NotNullable()
                .WithColumn("EndTime").AsDateTime().NotNullable();

            Execute.EmbeddedScript("BeginSession_M00001.sql");
            Execute.EmbeddedScript("EndSession_M00001.sql");
            Execute.EmbeddedScript("GetApplicationLabels_M00001.sql");
            Execute.EmbeddedScript("GetConfigSettings_M00001.sql");
            Execute.EmbeddedScript("GetSession_M00001.sql");
            Execute.EmbeddedScript("SelectSessions_M00001.sql");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

    }
}
