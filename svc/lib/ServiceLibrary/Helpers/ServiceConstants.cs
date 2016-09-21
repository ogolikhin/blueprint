using System.Configuration;

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

        public const int MaxSearchItems = 1000;

        public const int SearchPageSize = 10;
    }
}
