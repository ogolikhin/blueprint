using System.Collections.Generic;

namespace ServiceLibrary.Models.VersionControl
{
    public class SqlPublishResult
    {
        #region Public Classes

        //public class SqlPublishResultReply
        //{
        //    public int CommentId
        //    {
        //        get;
        //        internal set;
        //    }

        //    public int ReplyId
        //    {
        //        get;
        //        internal set;
        //    }

        //    public MentionChangeDetails MentionChangeDetails
        //    {
        //        get;
        //        internal set;
        //    }
        //}

        //public class SqlPublishResultComment
        //{
        //    public int CommentId
        //    {
        //        get;
        //        internal set;
        //    }

        //    public MentionChangeDetails MentionChangeDetails
        //    {
        //        get;
        //        internal set;
        //    }
        //}

        /// <summary>
        ///
        /// </summary>
        public class SqlPublishNewGuest
        {
            /// <summary>
            ///
            /// </summary>
            public int UserId
            {
                get;
                set;
            }

            /// <summary>
            ///
            /// </summary>
            public string Email
            {
                get;
                set;
            }
        }


        #endregion Public Classes

        #region Constructors

        /// <summary>
        ///
        /// </summary>
        public SqlPublishResult(int lockableId)
        {
            this.LockableId = lockableId;
        }
        #endregion Constructors

        #region Public Properties

        /// <summary>
        ///
        /// </summary>
        public bool Changed
        {
            get;
            private set;
        }

        /// <summary>
        ///
        /// </summary>
        public int LockableId
        {
            get;
            set;
        }

        //private ICollection<SqlPublishResultComment> comments;
        //public ICollection<SqlPublishResultComment> Comments
        //{
        //    get
        //    {
        //        if (comments == null)
        //        {
        //            Changed = true;
        //            comments = new List<SqlPublishResultComment>();
        //        }
        //        return comments;
        //    }
        //}

        //private ICollection<SqlPublishResultReply> replies;
        //public ICollection<SqlPublishResultReply> Replies
        //{
        //    get
        //    {
        //        if (replies == null)
        //        {
        //            Changed = true;
        //            replies = new List<SqlPublishResultReply>();
        //        }
        //        return replies;
        //    }
        //}

        /// <summary>
        ///
        /// </summary>
        private ICollection<SqlPublishNewGuest> _newGuests;
        public ICollection<SqlPublishNewGuest> NewGuests
        {
            get
            {
                if (_newGuests == null)
                {
                    Changed = true;
                    _newGuests = new List<SqlPublishNewGuest>();
                }
                return _newGuests;
            }
        }

        //private ICollection<ReviewChangeDetails> _reviewChangeDetailsList;
        //public ICollection<ReviewChangeDetails> ReviewChangeDetailsList
        //{
        //    get
        //    {
        //        if (_reviewChangeDetailsList == null)
        //        {
        //            Changed = true;
        //            _reviewChangeDetailsList = new List<ReviewChangeDetails>();
        //        }
        //        return _reviewChangeDetailsList;
        //    }
        //}
        #endregion Public Properties
    }
}
