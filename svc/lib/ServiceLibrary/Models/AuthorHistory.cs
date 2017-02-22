using System;

namespace ServiceLibrary.Models
{
    public class AuthorHistory
    {
        public int ItemId { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? LastEditedBy { get; set; }

        public DateTime? LastEditedOn { get; set; }
    }

    public class SqlAuthorHistory
    {
        public int ItemId { get; set; }

        public int? CreationUserId { get; set; }

        public DateTime? CreationTimestamp { get; set; }

        public int? ModificationUserId { get; set; }

        public DateTime? ModificationTimestamp { get; set; }

        public static explicit operator AuthorHistory(SqlAuthorHistory sqlAuthorHistory)
        {
            var authorHistory = new AuthorHistory
            {
                ItemId = sqlAuthorHistory.ItemId,
                CreatedBy = sqlAuthorHistory.CreationUserId,
                CreatedOn = ToUtc(sqlAuthorHistory.CreationTimestamp),
                LastEditedBy = sqlAuthorHistory.ModificationUserId,
                LastEditedOn = ToUtc(sqlAuthorHistory.ModificationTimestamp)
            };

            return authorHistory;
        }

        private static DateTime? ToUtc(DateTime? dateTime)
        {
            return dateTime.HasValue ? DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc) : (DateTime?)null;
        }

    }
}
