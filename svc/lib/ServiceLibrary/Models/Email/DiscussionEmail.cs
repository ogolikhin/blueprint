using System.Collections.Generic;
using System.Web;

namespace ServiceLibrary.Models.Email
{
    /// <summary>
    ///
    /// </summary>
    public class DiscussionEmail
    {
        #region Public Classes

        /// <summary>
        ///
        /// </summary>
        public class DDiscussionEmailReply
        {
            /// <summary>
            ///
            /// </summary>
            private string user;
            public string User
            {
                get
                {
                    return user;
                }
                internal set
                {
                    user = HttpUtility.HtmlEncode(value);
                }
            }

            /// <summary>
            ///
            /// </summary>
            public string Version
            {
                get;
                internal set;
            }

            /// <summary>
            ///
            /// </summary>
            public string Timestamp
            {
                get;
                internal set;
            }

            /// <summary>
            ///
            /// </summary>
            public string Body
            {
                get;
                internal set;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public class DDiscussionEmailComment : DDiscussionEmailReply
        {
            /// <summary>
            ///
            /// </summary>
            public string Status
            {
                get;
                internal set;
            }

            /// <summary>
            ///
            /// </summary>
            public bool IsClosed
            {
                get;
                internal set;
            }
        }
        #endregion Public Classes

        #region Public Properties
        #region Image Properties

        /// <summary>
        ///
        /// </summary>
        public static readonly string LogoImageAttachmentContentId = "LogoImage.png";

        /// <summary>
        ///
        /// </summary>
        public static readonly string ArtifactImageAttachmentContentId = "ArtifactImage.png";

        /// <summary>
        ///
        /// </summary>
        public byte[] LogoImageAttachmentArray
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public byte[] ArtifactImageAttachmentArray
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        public string LogoImageSrc
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public string ArtifactImageSrc
        {
            get;
            internal set;
        }

        #endregion Image Properties

        /// <summary>
        ///
        /// </summary>
        public DDiscussionEmailComment Comment
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        public ICollection<DDiscussionEmailReply> Replies
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        private string publisher;
        public string Publisher
        {
            get
            {
                return publisher;
            }
            internal set
            {
                publisher = HttpUtility.HtmlEncode(value);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string ProjectId
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        private string projectName;
        public string ProjectName
        {
            get
            {
                return projectName;
            }
            internal set
            {
                projectName = HttpUtility.HtmlEncode(value);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string ArtifactUrl
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        public string ArtifactId
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        private string artifactName;
        public string ArtifactName
        {
            get
            {
                return artifactName;
            }
            internal set
            {
                artifactName = HttpUtility.HtmlEncode(value);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string ArtifactBody
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        public string DiscussionBody
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        public int MaxCommentLength
        {
            get;
            internal set;
        }

        #endregion Public Properties
    }
}
