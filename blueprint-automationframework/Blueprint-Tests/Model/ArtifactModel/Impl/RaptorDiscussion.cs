using System;
using System.Collections.Generic;
using Model.ArtifactModel.Adaptors;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Model.ArtifactModel.Impl
{
    // Found in:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/RapidReview/Models/DiscussionsInfo.cs
    public class RaptorDiscussionsInfo : IRaptorDiscussionsInfo
    {
        #region Serialized JSON properties

        public int ArtifactId { get; set; }
        public int SubArtifactId { get; set; }
        public bool CanCreate { get; set; }
        public bool AreEmailDiscusssionsEnabled { get; set; }
        public List<IRaptorDiscussion> Discussions { get; } = new List<IRaptorDiscussion>();

        #endregion Serialized JSON properties

        public RaptorDiscussionsInfo()
        { }

        /// <summary>
        /// Asserts that all the properties of the two DiscussionsInfo are equal.
        /// </summary>
        /// <param name="expectedDiscussionInfo">The expected comment.</param>
        /// <param name="actualDiscussionInfo">The actual comment.</param>
        public static void AssertAreEqual(IRaptorDiscussionsInfo expectedDiscussionInfo, IRaptorDiscussionsInfo actualDiscussionInfo)
        {
            if ((expectedDiscussionInfo == null) || (actualDiscussionInfo == null))
            {
                Assert.AreEqual(expectedDiscussionInfo, actualDiscussionInfo, "One Discussion was null but the other wasn't!");
            }
            else
            {
                const string MESSAGE = "The {0} properties don't match!";

                Assert.AreEqual(expectedDiscussionInfo.AreEmailDiscusssionsEnabled, actualDiscussionInfo.AreEmailDiscusssionsEnabled, MESSAGE, nameof(IRaptorDiscussionsInfo.AreEmailDiscusssionsEnabled));
                Assert.AreEqual(expectedDiscussionInfo.ArtifactId, actualDiscussionInfo.ArtifactId, MESSAGE, nameof(IRaptorDiscussionsInfo.ArtifactId));
                Assert.AreEqual(expectedDiscussionInfo.CanCreate, actualDiscussionInfo.CanCreate, MESSAGE, nameof(IRaptorDiscussionsInfo.CanCreate));
                Assert.AreEqual(expectedDiscussionInfo.SubArtifactId, actualDiscussionInfo.SubArtifactId, MESSAGE, nameof(IRaptorDiscussionsInfo.SubArtifactId));

                // Compare Discussions.
                if ((expectedDiscussionInfo.Discussions == null) || (actualDiscussionInfo.Discussions == null))
                {
                    Assert.AreEqual(expectedDiscussionInfo.Discussions, actualDiscussionInfo.Discussions,
                        "One Discussions property was null but the other wasn't!");
                }
                else
                {
                    Assert.AreEqual(expectedDiscussionInfo.Discussions.Count, actualDiscussionInfo.Discussions.Count, "The number of Discussions are different!");

                    foreach (var expectedDiscussion in expectedDiscussionInfo.Discussions)
                    {
                        var actualDiscussion =
                            actualDiscussionInfo.Discussions.Find(
                                d => d.DiscussionId.Equals(expectedDiscussion.DiscussionId) && d.ItemId.Equals(expectedDiscussion.ItemId));

                        Assert.NotNull(actualDiscussion, "Couldn't find a Discussion with DiscussionId: {0} and ItemId: {1}",
                            expectedDiscussion.DiscussionId, expectedDiscussion.ItemId);

                        RaptorDiscussion.AssertAreEqual(expectedDiscussion, actualDiscussion);
                    }
                }
            }
        }
    }

    // Found in:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/RapidReview/Models/DiscussionsInfo.cs
    public class RaptorDiscussion : IRaptorDiscussion
    {
        #region Serialized JSON properties

        public List<IRaptorReply> Replies { get; set; }
        public bool IsClosed { get; set; }
        public int ItemId { get; set; }
        public int DiscussionId { get; set; }
        public int Version { get; set; }
        public int UserId { get; set; }

        [JsonProperty("LastEditedOnUtc")]
        public DateTime LastEditedOn { get; set; }

        public string Username { get; set; }
        public bool IsGuest { get; set; }
        public string Comment { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }

        #endregion Serialized JSON properties

        public RaptorDiscussion()
        { }

        /// <summary>
        /// Asserts that all the properties of the two Discussions are equal.
        /// </summary>
        /// <param name="expectedDiscussion">The expected comment.</param>
        /// <param name="actualDiscussion">The actual comment.</param>
        public static void AssertAreEqual(IRaptorDiscussion expectedDiscussion, IRaptorDiscussion actualDiscussion)
        {
            AssertAreEqual((IDiscussionAdaptor)expectedDiscussion, (IDiscussionAdaptor)actualDiscussion);

            if ((expectedDiscussion != null) && (actualDiscussion != null))
            {
                // Compare Replies.
                if ((expectedDiscussion.Replies == null) || (actualDiscussion.Replies == null))
                {
                    Assert.AreEqual(expectedDiscussion.Replies, actualDiscussion.Replies,
                        "One Replies property was null but the other wasn't!");
                }
                else
                {
                    Assert.AreEqual(expectedDiscussion.Replies.Count, actualDiscussion.Replies.Count, "The number of Replies are different!");

                    foreach (var expectedReply in expectedDiscussion.Replies)
                    {
                        var actualReply =
                            actualDiscussion.Replies.Find(
                                r => r.DiscussionId.Equals(expectedReply.DiscussionId) && r.ItemId.Equals(expectedReply.ItemId));

                        Assert.NotNull(actualReply, "Couldn't find a Reply with DiscussionId: {0} and ItemId: {1}",
                            expectedReply.DiscussionId, expectedReply.ItemId);

                        RaptorReply.AssertAreEqual(expectedReply, actualReply);
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that all the properties of the two Discussions are equal.
        /// </summary>
        /// <param name="expectedDiscussion">The expected comment.</param>
        /// <param name="actualDiscussion">The actual comment.</param>
        /// <param name="skipCanDelete">(optional) Pass true to skip comparison of the CanDelete properties.</param>
        /// <param name="skipCanEdit">(optional) Pass true to skip comparison of the CanEdit properties.</param>
        public static void AssertAreEqual(IDiscussionAdaptor expectedDiscussion, IDiscussionAdaptor actualDiscussion,
            bool skipCanDelete = false, bool skipCanEdit = false)
        {
            if ((expectedDiscussion == null) || (actualDiscussion == null))
            {
                Assert.AreEqual(expectedDiscussion, actualDiscussion, "One Discussion was null but the other wasn't!");
            }
            else
            {
                const string MESSAGE = "The {0} properties don't match!";

                if (!skipCanDelete)
                {
                    Assert.AreEqual(expectedDiscussion.CanDelete, actualDiscussion.CanDelete, MESSAGE, nameof(IDiscussionAdaptor.CanDelete));
                }

                if (!skipCanEdit)
                {
                    Assert.AreEqual(expectedDiscussion.CanEdit, actualDiscussion.CanEdit, MESSAGE, nameof(IDiscussionAdaptor.CanEdit));
                }

                // TFS Bug: 4706  Milliseconds are different.
                // We need to remove the milliseconds when comparing the dates because RapidReview and Nova return different milliseconds for some reason.
                var expectedLastEditedOn = expectedDiscussion.LastEditedOn;
                var actualLastEditedOn = actualDiscussion.LastEditedOn;

                expectedLastEditedOn = new DateTime(expectedLastEditedOn.Year, expectedLastEditedOn.Month, expectedLastEditedOn.Day,
                    expectedLastEditedOn.Hour, expectedLastEditedOn.Minute, expectedLastEditedOn.Second, expectedLastEditedOn.Kind);

                actualLastEditedOn = new DateTime(actualLastEditedOn.Year, actualLastEditedOn.Month, actualLastEditedOn.Day,
                    actualLastEditedOn.Hour, actualLastEditedOn.Minute, actualLastEditedOn.Second, actualLastEditedOn.Kind);

                Assert.AreEqual(expectedLastEditedOn, actualLastEditedOn, MESSAGE, nameof(IDiscussionAdaptor.LastEditedOn));

                Assert.AreEqual(expectedDiscussion.Comment, actualDiscussion.Comment, MESSAGE, nameof(IDiscussionAdaptor.Comment));
                Assert.AreEqual(expectedDiscussion.DiscussionId, actualDiscussion.DiscussionId, MESSAGE, nameof(IDiscussionAdaptor.DiscussionId));
                Assert.AreEqual(expectedDiscussion.IsClosed, actualDiscussion.IsClosed, MESSAGE, nameof(IDiscussionAdaptor.IsClosed));
                Assert.AreEqual(expectedDiscussion.IsGuest, actualDiscussion.IsGuest, MESSAGE, nameof(IDiscussionAdaptor.IsGuest));
                Assert.AreEqual(expectedDiscussion.ItemId, actualDiscussion.ItemId, MESSAGE, nameof(IDiscussionAdaptor.ItemId));
//                Assert.AreEqual(expectedDiscussion.LastEditedOn, actualDiscussion.LastEditedOn, MESSAGE, nameof(IDiscussionAdaptor.LastEditedOn));
                Assert.AreEqual(expectedDiscussion.UserId, actualDiscussion.UserId, MESSAGE, nameof(IDiscussionAdaptor.UserId));
                Assert.AreEqual(expectedDiscussion.Username, actualDiscussion.Username, MESSAGE, nameof(IDiscussionAdaptor.Username));
                Assert.AreEqual(expectedDiscussion.Version, actualDiscussion.Version, MESSAGE, nameof(IDiscussionAdaptor.Version));
            }
        }

        public bool Equals(Discussion comment)
        {
            if (comment == null)
            { return false; }
            else
            {
                return (string.Equals(comment.Comment, Comment)) &&
                    (comment.DiscussionId == DiscussionId) && (comment.IsClosed == IsClosed) &&
                    (comment.IsGuest == IsGuest) && (comment.ItemId == ItemId) &&
                    //(DateTime.Equals(comment.LastEditedOn, LastEditedOnUtc)) && //microseconds are different
                    (comment.UserId == UserId) && (string.Equals(comment.Username, Username)) &&
                    (comment.Version == Version) && (comment.CanDelete == CanDelete) &&
                    (comment.CanEdit == CanEdit);
            }
        }
    }

    // Found in:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/RapidReview/Models/DiscussionsInfo.cs
    public class RaptorReply : IRaptorReply
    {
        #region Serialized JSON properties

        public int ReplyId { get; set; }
        public int ItemId { get; set; }
        public int DiscussionId { get; set; }
        public int Version { get; set; }
        public int UserId { get; set; }
        public DateTime LastEditedOnUtc { get; set; }
        public string UserName { get; set; }
        public bool IsGuest { get; set; }
        public string Comment { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }

        #endregion Serialized JSON properties

        /// <summary>
        /// Asserts that all the properties of the two Replies are equal.
        /// </summary>
        /// <param name="expectedReply">The expected Reply.</param>
        /// <param name="actualReply">The actual Reply.</param>
        public static void AssertAreEqual(IRaptorReply expectedReply, IRaptorReply actualReply)
        {
            if ((expectedReply == null) || (actualReply == null))
            {
                Assert.AreEqual(expectedReply, actualReply, "One Reply was null but the other wasn't!");
            }
            else
            {
                const string MESSAGE = "The {0} properties don't match!";

                Assert.AreEqual(expectedReply.CanDelete, actualReply.CanDelete, MESSAGE, nameof(IRaptorReply.CanDelete));
                Assert.AreEqual(expectedReply.CanEdit, actualReply.CanEdit, MESSAGE, nameof(IRaptorReply.CanEdit));
                Assert.AreEqual(expectedReply.Comment, actualReply.Comment, MESSAGE, nameof(IRaptorReply.Comment));
                Assert.AreEqual(expectedReply.DiscussionId, actualReply.DiscussionId, MESSAGE, nameof(IRaptorReply.DiscussionId));
                Assert.AreEqual(expectedReply.IsGuest, actualReply.IsGuest, MESSAGE, nameof(IRaptorReply.IsGuest));
                Assert.AreEqual(expectedReply.ItemId, actualReply.ItemId, MESSAGE, nameof(IRaptorReply.ItemId));
                Assert.AreEqual(expectedReply.LastEditedOnUtc, actualReply.LastEditedOnUtc, MESSAGE, nameof(IRaptorReply.LastEditedOnUtc));
                Assert.AreEqual(expectedReply.ReplyId, actualReply.ReplyId, MESSAGE, nameof(IRaptorReply.ReplyId));
                Assert.AreEqual(expectedReply.UserId, actualReply.UserId, MESSAGE, nameof(IRaptorReply.UserId));
                Assert.AreEqual(expectedReply.UserName, actualReply.UserName, MESSAGE, nameof(IRaptorReply.UserName));
                Assert.AreEqual(expectedReply.Version, actualReply.Version, MESSAGE, nameof(IRaptorReply.Version));
            }
        }

        public bool Equals(Reply reply)
        {
            if (reply == null)
            { return false; }
            else
            {
                if ((string.Equals(reply.ReplyText, Comment)) &&
                    (reply.DiscussionId == DiscussionId) && (reply.IsGuest == IsGuest) &&
                    (reply.ItemId == ItemId) && (reply.ReplyId == ReplyId) &&
                    //(DateTime.Equals(comment.LastEditedOn, LastEditedOnUtc)) && //microseconds are different
                    (reply.UserId == UserId) && (string.Equals(reply.UserName, UserName)))
                { return true; }
                else
                { return false; }
            }
        }
    }
}
