using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace HtmlLibrary.Tests
{
    [TestClass]
    public class MentionProcessorTests
    {
        private IMentionValidator mentionHelper;
        private MentionProcessor mentionProcessor;

        [TestInitialize]
        public void init()
        {
            mentionHelper = new MentionHelperMock();
            mentionProcessor = new MentionProcessor(mentionHelper);
        }

        [TestMethod]
        public async Task ProcessComment_DiscussionsDisabled_CommentProcessed()
        {
            // Arrange
            string comment = "<html><a linkassemblyqualifiedname=\"BluePrintSys.RC.Client.SL.RichText.RichTextMentionLink, BluePrintSys.RC.Client.SL.RichText, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\" isvalid=\"True\" mentionid=\"5\" isgroup=\"False\" email=\"user@user.com\" style=\"-c1-editable: false; font-size: 11px; line-height: 1.45000004768372\"><span style=\"font-size: 11px; font-weight: normal; font-family: &#39;Portable User Interface&#39;; line-height: 1.45000004768372\">user@user.com</span></a></html>";
            bool areEmailDiscussionsEnabled = false;

            // Act
            var result = await mentionProcessor.ProcessComment(comment, areEmailDiscussionsEnabled);

            // Assert
            //Assert.AreEqual(expectedProcessedComment, comment);
        }
        [TestMethod]
        public async Task ProcessComment_DiscussionsEnabled_CommentProcessed()
        {
            // Arrange
            string comment = "<html><a linkassemblyqualifiedname=\"BluePrintSys.RC.Client.SL.RichText.RichTextMentionLink, BluePrintSys.RC.Client.SL.RichText, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\" isvalid=\"True\" mentionid=\"5\" isgroup=\"False\" email=\"user@user.com\" style=\"-c1-editable: false; font-size: 11px; line-height: 1.45000004768372\"><span style=\"font-size: 11px; font-weight: normal; font-family: &#39;Portable User Interface&#39;; line-height: 1.45000004768372\">user@user.com</span></a></html>";
            bool areEmailDiscussionsEnabled = true;

            // Act
            var result = await mentionProcessor.ProcessComment(comment, areEmailDiscussionsEnabled);
        }
    }
}
