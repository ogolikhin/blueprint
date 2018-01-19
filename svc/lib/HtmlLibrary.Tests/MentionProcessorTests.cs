using System;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HtmlLibrary
{
    [TestClass]
    public class MentionProcessorTests
    {
        private Mock<IMentionValidator> _mentionHelperMock;
        private MentionProcessor _mentionProcessor;

        [TestInitialize]
        public void Initialize()
        {
            _mentionHelperMock = new Mock<IMentionValidator>();
            _mentionProcessor = new MentionProcessor(_mentionHelperMock.Object);
        }

        [TestMethod]
        public async Task ProcessComment_DiscussionsDisabled_TooltipAdded()
        {
            // Arrange
            const string comment = "<html><a linkassemblyqualifiedname=\"BluePrintSys.RC.Client.SL.RichText.RichTextMentionLink, BluePrintSys.RC.Client.SL.RichText, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\" isvalid=\"True\" mentionid=\"5\" isgroup=\"False\" email=\"user@user.com\" style=\"-c1-editable: false; font-size: 11px; line-height: 1.45000004768372\"><span style=\"font-size: 11px; font-weight: normal; font-family: &#39;Portable User Interface&#39;; line-height: 1.45000004768372\">user@user.com</span></a></html>";
            const bool areEmailDiscussionsEnabled = false;
            _mentionHelperMock.Setup(a => a.IsEmailBlocked("user@user.com")).Returns(Task.FromResult(false));

            // Act
            var result = await _mentionProcessor.ProcessComment(comment, areEmailDiscussionsEnabled);
            var xDoc = new HtmlDocument();
            xDoc.LoadHtml(result);
            var mentions =
                (from e in xDoc.DocumentNode.Descendants()
                where e.Attributes.Count(a => a.Name.Equals("linkassemblyqualifiedname", StringComparison.CurrentCultureIgnoreCase) && a.Value.Contains("RichTextMentionLink")) > 0
                select e).ToList();

            // Assert
            Assert.AreEqual(1, mentions.Count);
            Assert.AreEqual("Email Discussions have been Disabled", mentions.Single().GetAttributeValue("title", null));
        }

        [TestMethod]
        public async Task ProcessComment_EmailBlocked_TooltipAdded()
        {
            // Arrange
            const string comment = "<html><a linkassemblyqualifiedname=\"BluePrintSys.RC.Client.SL.RichText.RichTextMentionLink, BluePrintSys.RC.Client.SL.RichText, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\" isvalid=\"True\" mentionid=\"5\" isgroup=\"False\" email=\"blocked@blocked.com\" style=\"-c1-editable: false; font-size: 11px; line-height: 1.45000004768372\"><span style=\"font-size: 11px; font-weight: normal; font-family: &#39;Portable User Interface&#39;; line-height: 1.45000004768372\">blocked@blocked.com</span></a></html>";
            _mentionHelperMock.Setup(a => a.IsEmailBlocked("blocked@blocked.com")).Returns(Task.FromResult(true));
            const bool areEmailDiscussionsEnabled = true;

            // Act
            var result = await _mentionProcessor.ProcessComment(comment, areEmailDiscussionsEnabled);
            var xDoc = new HtmlDocument();
            xDoc.LoadHtml(result);
            var mentions =
                (from e in xDoc.DocumentNode.Descendants()
                where e.Attributes.Count(a => a.Name.Equals("linkassemblyqualifiedname", StringComparison.CurrentCultureIgnoreCase) && a.Value.Contains("RichTextMentionLink")) > 0
                select e).ToList();

            // Assert
            Assert.AreEqual(1, mentions.Count());
            Assert.AreEqual("Email is blocked by Instance Admin", mentions.Single().GetAttributeValue("title", null));
        }

        [TestMethod]
        public async Task ProcessComment_DiscussionsEnabled_NoTooltipCommentProcessed()
        {
            // Arrange
            const string comment = "<html><a linkassemblyqualifiedname=\"BluePrintSys.RC.Client.SL.RichText.RichTextMentionLink, BluePrintSys.RC.Client.SL.RichText, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\" isvalid=\"True\" mentionid=\"5\" isgroup=\"False\" email=\"user@user.com\" style=\"-c1-editable: false; font-size: 11px; line-height: 1.45000004768372\"><span style=\"font-size: 11px; font-weight: normal; font-family: &#39;Portable User Interface&#39;; line-height: 1.45000004768372\">user@user.com</span></a></html>";
            const bool areEmailDiscussionsEnabled = true;
            _mentionHelperMock.Setup(a => a.IsEmailBlocked("user@user.com")).Returns(Task.FromResult(false));

            // Act
            var result = await _mentionProcessor.ProcessComment(comment, areEmailDiscussionsEnabled);
            var xDoc = new HtmlDocument();
            xDoc.LoadHtml(result);
            var mentions =
                (from e in xDoc.DocumentNode.Descendants()
                where e.Attributes.Count(a => a.Name.Equals("linkassemblyqualifiedname", StringComparison.CurrentCultureIgnoreCase) && a.Value.Contains("RichTextMentionLink")) > 0
                select e).ToList();

            // Assert
            Assert.AreEqual(1, mentions.Count());
            Assert.IsNull(mentions.Single().GetAttributeValue("tooltip", null));
        }
    }
}
