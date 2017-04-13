﻿using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Model.ArtifactModel.Adapters;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    // Found in:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/RapidReview/Models/DiscussionsInfo.cs
    public class RaptorDiscussionsInfo : IRaptorDiscussionsInfo
    {
        #region Serialized JSON properties

        public int ArtifactId { get; set; }
        public int? SubArtifactId { get; set; }
        public bool CanCreate { get; set; }
        public bool AreEmailDiscusssionsEnabled { get; set; }
        public List<IRaptorDiscussion> Discussions { get; } = new List<IRaptorDiscussion>();

        #endregion Serialized JSON properties

        public RaptorDiscussionsInfo()
        { }

        /// <summary>
        /// Asserts that all the properties of the two DiscussionsInfo are equal.
        /// </summary>
        /// <param name="expectedDiscussionInfo">The expected DiscussionInfo.</param>
        /// <param name="actualDiscussionInfo">The actual DiscussionInfo.</param>
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

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public List<IReplyAdapter> Replies { get; set; }

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
        /// <param name="expectedDiscussion">The expected discussion.</param>
        /// <param name="actualDiscussion">The actual discussion.</param>
        public static void AssertAreEqual(IRaptorDiscussion expectedDiscussion, IRaptorDiscussion actualDiscussion)
        {
            AssertAreEqual((IDiscussionAdapter)expectedDiscussion, (IDiscussionAdapter)actualDiscussion);

            if ((expectedDiscussion != null) && (actualDiscussion != null))
            {
                Assert.AreEqual(expectedDiscussion.Username, actualDiscussion.Username, "The {0} properties don't match!", nameof(IRaptorDiscussion.Username));

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
        /// <param name="expectedDiscussion">The expected discussion.</param>
        /// <param name="actualDiscussion">The actual discussion.</param>
        /// <param name="skipCanDelete">(optional) Pass true to skip comparison of the CanDelete properties.</param>
        /// <param name="skipCanEdit">(optional) Pass true to skip comparison of the CanEdit properties.</param>
        public static void AssertAreEqual(IDiscussionAdapter expectedDiscussion, IDiscussionAdapter actualDiscussion,
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
                    Assert.AreEqual(expectedDiscussion.CanDelete, actualDiscussion.CanDelete, MESSAGE, nameof(IDiscussionAdapter.CanDelete));
                }

                if (!skipCanEdit)
                {
                    Assert.AreEqual(expectedDiscussion.CanEdit, actualDiscussion.CanEdit, MESSAGE, nameof(IDiscussionAdapter.CanEdit));
                }

                // TFS Bug: 4706  Milliseconds are different.
                // When comparing the dates because RapidReview and Nova return different milliseconds for some reason, so just verify that they're less than 1 second apart.
                Assert.That(expectedDiscussion.LastEditedOn.CompareTimePlusOrMinusMilliseconds(actualDiscussion.LastEditedOn, 1000),
                    MESSAGE, nameof(IDiscussionAdapter.LastEditedOn));

                Assert.AreEqual(expectedDiscussion.Comment, actualDiscussion.Comment, MESSAGE, nameof(IDiscussionAdapter.Comment));
                Assert.AreEqual(expectedDiscussion.DiscussionId, actualDiscussion.DiscussionId, MESSAGE, nameof(IDiscussionAdapter.DiscussionId));
                Assert.AreEqual(expectedDiscussion.IsClosed, actualDiscussion.IsClosed, MESSAGE, nameof(IDiscussionAdapter.IsClosed));
                Assert.AreEqual(expectedDiscussion.IsGuest, actualDiscussion.IsGuest, MESSAGE, nameof(IDiscussionAdapter.IsGuest));
                Assert.AreEqual(expectedDiscussion.ItemId, actualDiscussion.ItemId, MESSAGE, nameof(IDiscussionAdapter.ItemId));
                Assert.AreEqual(expectedDiscussion.UserId, actualDiscussion.UserId, MESSAGE, nameof(IDiscussionAdapter.UserId));
                Assert.AreEqual(expectedDiscussion.Version, actualDiscussion.Version, MESSAGE, nameof(IDiscussionAdapter.Version));
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

        [JsonProperty("LastEditedOnUtc")]
        public DateTime LastEditedOn { get; set; }

        public string Username { get; set; }
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
        /// <param name="skipCanEdit">(optional) Pass true to skip comparison of the CanEdit properties.</param>
        public static void AssertAreEqual(IRaptorReply expectedReply, IRaptorReply actualReply, bool skipCanEdit = false)
        {
            AssertAreEqual((IReplyAdapter)expectedReply, (IReplyAdapter)actualReply, skipCanEdit);

            if ((expectedReply != null) && (actualReply != null))
            {
                Assert.AreEqual(expectedReply.Username, actualReply.Username, "The {0} properties don't match!", nameof(IRaptorReply.Username));
            }
        }

        /// <summary>
        /// Asserts that all the properties of the two Replies are equal.
        /// </summary>
        /// <param name="expectedReply">The expected Reply.</param>
        /// <param name="actualReply">The actual Reply.</param>
        /// <param name="skipCanEdit">(optional) Pass true to skip comparison of the CanEdit properties.</param>
        public static void AssertAreEqual(IReplyAdapter expectedReply, IReplyAdapter actualReply, bool skipCanEdit = false)
        {
            if ((expectedReply == null) || (actualReply == null))
            {
                Assert.AreEqual(expectedReply, actualReply, "One Reply was null but the other wasn't!");
            }
            else
            {
                const string MESSAGE = "The {0} properties don't match!";

                Assert.AreEqual(expectedReply.CanDelete, actualReply.CanDelete, MESSAGE, nameof(IReplyAdapter.CanDelete));

                if (!skipCanEdit)
                {
                    Assert.AreEqual(expectedReply.CanEdit, actualReply.CanEdit, MESSAGE, nameof(IReplyAdapter.CanEdit));
                }

                // TFS Bug: 4706  Milliseconds are different.
                // When comparing the dates because RapidReview and Nova return different milliseconds for some reason, so just verify that they're less than 1 second apart.
                var timeSpan = new TimeSpan(Math.Abs(expectedReply.LastEditedOn.Ticks - actualReply.LastEditedOn.Ticks));
                Assert.Less(timeSpan.TotalMilliseconds, 1000, MESSAGE, nameof(IDiscussionAdapter.LastEditedOn));

                Assert.AreEqual(expectedReply.Comment, actualReply.Comment, MESSAGE, nameof(IReplyAdapter.Comment));
                Assert.AreEqual(expectedReply.DiscussionId, actualReply.DiscussionId, MESSAGE, nameof(IReplyAdapter.DiscussionId));
                Assert.AreEqual(expectedReply.IsGuest, actualReply.IsGuest, MESSAGE, nameof(IReplyAdapter.IsGuest));
                Assert.AreEqual(expectedReply.ItemId, actualReply.ItemId, MESSAGE, nameof(IReplyAdapter.ItemId));
                Assert.AreEqual(expectedReply.ReplyId, actualReply.ReplyId, MESSAGE, nameof(IReplyAdapter.ReplyId));
                Assert.AreEqual(expectedReply.UserId, actualReply.UserId, MESSAGE, nameof(IReplyAdapter.UserId));
                Assert.AreEqual(expectedReply.Version, actualReply.Version, MESSAGE, nameof(IReplyAdapter.Version));
            }
        }
    }
}
