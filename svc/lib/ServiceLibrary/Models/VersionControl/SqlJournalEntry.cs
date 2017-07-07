using System;

namespace ServiceLibrary.Models.VersionControl
{
    public class SqlJournalEntry
    {
        public SqlJournalEntry()
        {
            RevisionID = 0;
            UserID = 0;
            UserDisplayName = string.Empty;
            ElementType = 0;
            ItemID = 0;
            SubItemID = 0;
            ProjectID = 0;
            ItemDescription = string.Empty;
            ItemTypePrefix = string.Empty;
            TimeStamp = new DateTime();
            ThreadID = 0;
            ThreadDescription = string.Empty;
            CommentID = 0;
            CommentDescription = string.Empty;
            ActionType = 0;
            SubActionType = 0;
            AdditionalInfoNum = 0;
            AdditionalInfoTxt = string.Empty;
        }

        public int RevisionID { get; set; }
        public int UserID { get; set; }
        public string UserDisplayName { get; set; }
        public int ElementType { get; set; }
        public int ItemID { get; set; }
        public int SubItemID { get; set; }
        public int ProjectID { get; set; }
        public string ItemDescription { get; set; }
        public string ItemTypePrefix { get; set; }
        public DateTime TimeStamp { get; set; }
        public int ThreadID { get; set; }
        public string ThreadDescription { get; set; }
        public int CommentID { get; set; }
        public string CommentDescription { get; set; }
        public int ActionType { get; set; }
        public int SubActionType { get; set; }
        public int AdditionalInfoNum { get; set; }
        public string AdditionalInfoTxt { get; set; }
    }
}
