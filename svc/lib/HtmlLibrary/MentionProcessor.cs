using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HtmlLibrary
{
    public class MentionProcessor
    {
        internal const string MENTION_LINK_OBJECT_NAME = "LinkAssemblyQualifiedName";
        internal const string MENTION_LINK_OBJECT = "RichTextMentionLink";
        internal const string MENTION_EMAIL_ATTRIBUTE = "Email";

        private readonly IMentionValidator _mentionValidator;

        public MentionProcessor(IMentionValidator mentionValidator)
        {
            _mentionValidator = mentionValidator;
        }

        public async Task<string> ProcessComment(string comment, bool areEmailDiscussionsEnabled)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return comment;
            }
            var xDoc = new HtmlDocument();
            xDoc.LoadHtml(comment);
            IEnumerable<HtmlNode> mentions =
                from e in xDoc.DocumentNode.Descendants()
                where e.Attributes.Count(a => a.Name.Equals(MENTION_LINK_OBJECT_NAME, StringComparison.CurrentCultureIgnoreCase) && a.Value.Contains(MENTION_LINK_OBJECT)) > 0
                select e;
            foreach (var mention in mentions)
            {
                var emailAttr = mention.Attributes.FirstOrDefault(a => a.Name.Equals(MENTION_EMAIL_ATTRIBUTE, StringComparison.CurrentCultureIgnoreCase));
                if (emailAttr != null)
                {
                    var email = emailAttr.Value == null ? string.Empty : emailAttr.Value.Trim();
                    var isEmailBlocked = await _mentionValidator.IsEmailBlocked(email);
                    var innerHtml = mention.InnerHtml;
                    var innerDoc = new HtmlDocument();
                    innerDoc.LoadHtml(innerHtml);
                    var spanNode = innerDoc.DocumentNode.ChildNodes.FirstOrDefault(a => a.Name.Equals("span"));
                    var styleAttribute = spanNode != null
                        ? spanNode.Attributes.FirstOrDefault(a => a.Name.Equals("style"))
                        : null;

                    if (styleAttribute != null)
                    {
                        var styleAttrValue = styleAttribute.Value;
                        var listOfStyles = styleAttrValue.Split(';');
                        var styleDictionary = new Dictionary<string, string>();
                        foreach (var pair in listOfStyles.Select(entry => entry.Split(':')).Where(pair => pair.Length == 2))
                        {
                            styleDictionary[pair[0].Trim()] = pair[1].Trim();
                        }
                        styleDictionary["font-style"] = "italic";
                        if (isEmailBlocked)
                        {
                            styleDictionary["font-weight"] = "normal";
                            mention.SetAttributeValue("title", "Email is blocked by Instance Admin");
                        }
                        else if (!areEmailDiscussionsEnabled)
                        {
                            styleDictionary["font-weight"] = "normal";
                            mention.SetAttributeValue("title", "Email Discussions have been Disabled");
                        }
                        else
                        {
                            styleDictionary["font-weight"] = "bold";
                            mention.Attributes.Remove("title");
                        }
                        var newStyleString = "";
                        foreach (var entry in styleDictionary)
                        {
                            var entryString = entry.Key + ": " + entry.Value + "; ";
                            newStyleString += entryString;
                        }
                        styleAttribute.Value = newStyleString;
                    }
                    mention.InnerHtml = spanNode != null ? spanNode.OuterHtml : "";
                }
            }
            return xDoc.DocumentNode.OuterHtml;
        }
    }
}
