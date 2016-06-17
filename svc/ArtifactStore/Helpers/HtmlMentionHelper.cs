using System;
using System.Collections.Generic;
using System.Linq;

namespace ArtifactStore.Helpers
{
    public class HtmlMentionHelper
    {
        public string ProcessComment(string comment, int itemId)
        {
            //var emailDiscussionsEnabled = AreEmailDiscussionsEnabled(itemId);
            //var instanceEmailSettings = _dataAccess.InstanceSettingsService.GetEmailSettings();
            //var xDoc = new HtmlDocument();
            //xDoc.LoadHtml(comment);
            //IEnumerable<HtmlNode> mentions =
            //    from e in xDoc.DocumentNode.Descendants()
            //    where e.Attributes.Count(a => a.Name.Equals(MentionChangeDetailsHelper.MENTION_LINK_OBJECT_NAME, StringComparison.CurrentCultureIgnoreCase) && a.Value.Contains(MentionChangeDetailsHelper.MENTION_LINK_OBJECT)) > 0
            //    select e;
            //foreach (var mention in mentions)
            //{
            //    var emailAttr = mention.Attributes.FirstOrDefault(a => a.Name.Equals(MentionChangeDetailsHelper.MENTION_EMAIL_ATTRIBUTE, StringComparison.CurrentCultureIgnoreCase));
            //    var email = emailAttr == null ? string.Empty : emailAttr.Value.Trim();
            //    var user = _dataAccess.UserService.GetUsersByEmail(email, true).SingleOrDefault();
            //    if (emailAttr != null)
            //    {
            //        var innerHtml = mention.InnerHtml;
            //        var innerDoc = new HtmlDocument();
            //        innerDoc.LoadHtml(innerHtml);
            //        var spanNode = innerDoc.DocumentNode.ChildNodes.FirstOrDefault(a => a.Name.Equals("span"));
            //        var styleAttribute = spanNode != null
            //            ? spanNode.Attributes.FirstOrDefault(a => a.Name.Equals("style"))
            //            : null;

            //        if (styleAttribute != null)
            //        {
            //            var styleAttrValue = styleAttribute.Value;
            //            var listOfStyles = styleAttrValue.Split(';');
            //            var styleDictionary = new Dictionary<string, string>();
            //            foreach (var pair in listOfStyles.Select(entry => entry.Split(':')).Where(pair => pair.Length == 2))
            //            {
            //                styleDictionary[pair[0].Trim()] = pair[1].Trim();
            //            }
            //            styleDictionary["font-style"] = "italic";
            //            if ((user != null) &&
            //                ((user.Guest && !user.Enabled) || (!MentionChangeDetailsHelper.CheckUsersEmailDomain(email, user.Enabled, user.Guest, instanceEmailSettings))))
            //            {
            //                styleDictionary["font-weight"] = "normal";
            //                mention.SetAttributeValue("tooltip", "Email is blocked by Instance Admin");
            //            }
            //            else if (!emailDiscussionsEnabled)
            //            {
            //                styleDictionary["font-weight"] = "normal";
            //                mention.SetAttributeValue("tooltip", "Email Discussions have been Disabled");
            //            }
            //            else
            //            {
            //                styleDictionary["font-weight"] = "bold";
            //                mention.Attributes.Remove("tooltip");
            //            }
            //            var newStyleString = "";
            //            foreach (var entry in styleDictionary)
            //            {
            //                var entryString = entry.Key + ": " + entry.Value + "; ";
            //                newStyleString += entryString;
            //            }
            //            styleAttribute.Value = newStyleString;
            //        }
            //        mention.InnerHtml = spanNode != null ? spanNode.OuterHtml : null;
            //    }
            //}
            //return xDoc.DocumentNode.OuterHtml;
            return comment;
        }
    }
}