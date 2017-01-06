﻿using System.Configuration;

namespace ServiceLibrary.Helpers
{
    public static class ServiceConstants
    {
        public const string SessionProperty = "Session";

        public const string ErrorCodeName = "ErrorCode";

        public static string RaptorMain = ConfigurationManager.ConnectionStrings["RaptorMain"].ConnectionString;

        // Revisions 
        public const int VersionHead = int.MaxValue;

        public const int VersionDraft = 1;

        public const int VersionDraftDeleted = -1;

        // Sync with Blueprint in BluePrintSys.RC.Business.Internal.Components.Shared.Helpers.BusinessApplicationConstants
        public const int StubProjectItemTypeId = -1;
        public const int StubCollectionsItemTypeId = -2;
        public const int StubBaselinesAndReviewsItemTypeId = -3;

        public const string DefaultDBSchema = "[dbo]";
        public const string FileStoreDBSchema = "[FileStore]";

        public const int MaxSearchItems = 500;

        public const int SearchPageSize = 10;

        public const int MaxSearchableValueStringSize = 250;

        public const int MinSearchQueryCharLimit = 3;

        public const int MaxSearchQueryCharLimit = 250;

        public const int DefaultSearchTimeout = 120;

        public const int JobsDetailsPageSize = 10;

        public const string NoPermissions = "<No Permission>";

        public const string BlueprintSessionTokenKey = "Session-Token";
        public const string BlueprintSessionIgnoreKey = "e51d8f58-0c62-46ad-a6fc-7e7994670f34"; // random guid generated as a bypass validation header
        public const string CookieBlueprintSessionTokenKey = "BLUEPRINT_SESSION_TOKEN";

        public const int DefaultRequestTimeout = 600000;
    }
}
