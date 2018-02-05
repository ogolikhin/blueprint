using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace HtmlLibrary.RichText
{
    public static class HtmlHelper
    {
        private const string ZeroWidthSpace = "&#x200b;";

        private static readonly HashSet<string> SpecialNodeNames =
            new HashSet<string> { "html", "body", "tr", "td", "th" };

        public static string ToPlainText(string html, PlainTextOptions options = PlainTextOptions.All,
            string newLineReplacement = null)
        {
            // Must return empty string in case of chained string manipulation
            if (string.IsNullOrEmpty(html))
            {
                return string.Empty;
            }

            if ((options & PlainTextOptions.Verify) != 0 && !IsHtml(html))
            {
                return html;
            }

            var result = ConvertToText(html, options);

            return newLineReplacement == null ? result : result.ReplaceNewLines(newLineReplacement);
        }

        private static bool IsHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            // Check for both encoded and decoded html
            var trimmedLoweredText = text.TrimStart().ToLower(CultureInfo.CurrentCulture);

            return trimmedLoweredText.StartsWith("&lt;html", StringComparison.OrdinalIgnoreCase) ||
                   trimmedLoweredText.StartsWith("<html", StringComparison.OrdinalIgnoreCase) ||
                   trimmedLoweredText.StartsWith("&lt;head", StringComparison.OrdinalIgnoreCase) ||
                   trimmedLoweredText.StartsWith("<head", StringComparison.OrdinalIgnoreCase) ||
                   trimmedLoweredText.StartsWith("&lt;body", StringComparison.OrdinalIgnoreCase) ||
                   trimmedLoweredText.StartsWith("<body", StringComparison.OrdinalIgnoreCase) ||
                   trimmedLoweredText.StartsWith("&lt;div", StringComparison.OrdinalIgnoreCase) ||
                   trimmedLoweredText.StartsWith("<div", StringComparison.OrdinalIgnoreCase);
        }

        private static string ConvertToText(string html, PlainTextOptions options)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            if (doc.ParseErrors.Any())
            {
                return html;
            }

            var result = ConvertToText(doc, options);

            // Trim result for TextOnly option
            return options == PlainTextOptions.TextOnly ? result.Trim() : result;
        }

        private static string ConvertToText(HtmlDocument doc, PlainTextOptions options)
        {
            using (var sw = new StringWriter(CultureInfo.CurrentCulture))
            {
                ConvertTo(doc.DocumentNode, sw, options);
                sw.Flush();

                return sw.ToString().TrimEnd();
            }
        }

        private static void ConvertTo(HtmlNode node, TextWriter outText, PlainTextOptions options)
        {
            ConvertTo(node, outText, new DomTextInfo(), options);
        }

        private static void ConvertContentTo(HtmlNode node, TextWriter outText, DomTextInfo textInfo,
            PlainTextOptions options)
        {
            foreach (var subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText, textInfo, options);
            }
        }

        private static void ConvertTo(HtmlNode node, TextWriter outText, DomTextInfo textInfo, PlainTextOptions options)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText, textInfo, options);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    var parentName = node.ParentNode.Name;

                    if (parentName == "script" || parentName == "style")
                    {
                        break;
                    }

                    var html = ((HtmlTextNode)node).Text;
                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                    {
                        break;
                    }

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Length == 0)
                    {
                        break;
                    }

                    if (textInfo.PreserveFormatting)
                    {
                        // Entitize (decode) text if needed
                        if ((options & PlainTextOptions.Entitize) != 0)
                        {
                            html = DecodeHtml(html);
                        }

                        outText.Write(html);
                        break;
                    }

                    if (!textInfo.WritePrecedingWhiteSpace || textInfo.LastCharWasSpace)
                    {
                        html = html.TrimStart();

                        if (html.Length == 0)
                        {
                            break;
                        }

                        textInfo.WritePrecedingWhiteSpace = true;
                    }

                    // special case to get rid of "zero width space"
                    // Entitize (decode) text if needed
                    outText.Write(
                        (options & PlainTextOptions.Entitize) != 0
                            ? Regex.Replace(DecodeHtml(html.TrimEnd()), @"\s{2,}", " ").Replace("\u200b", "")
                            : Regex.Replace(html.TrimEnd(), @"\s{2,}", " ").Replace(ZeroWidthSpace, ""));

                    textInfo.LastCharWasSpace = char.IsWhiteSpace(html[html.Length - 1]);

                    if (textInfo.LastCharWasSpace)
                    {
                        outText.Write(' ');
                    }

                    break;

                case HtmlNodeType.Element:
                    string endElementString = null;
                    bool isInline;
                    var skip = false;
                    var listIndex = 0;

                    switch (node.Name)
                    {
                        case "nav":
                            skip = true;
                            isInline = false;
                            break;
                        case "body":
                        case "section":
                        case "article":
                        case "aside":
                        case "h1":
                        case "h2":
                        case "header":
                        case "footer":
                        case "address":
                        case "main":
                        case "div":
                            if ((options & PlainTextOptions.Paragraph) != 0)
                            {
                                endElementString = "\r\n";
                                textInfo.IsNewLineAddedAtEnd = true;
                            }

                            isInline = false;
                            break;

                        case "p":
                            if ((options & PlainTextOptions.Paragraph) != 0)
                            {
                                AddNewLineAtStartIfRequired(node, outText, textInfo);

                                if (!IsLastChildNodeInSpecialNodes(node))
                                {
                                    endElementString = "\r\n";
                                    textInfo.IsNewLineAddedAtEnd = true;
                                }
                            }

                            isInline = false;
                            break;

                        case "tr":
                            if ((options & PlainTextOptions.Table) != 0)
                            {
                                endElementString = "\r\n";
                                textInfo.IsNewLineAddedAtEnd = true;
                            }

                            isInline = false;
                            break;

                        case "th": // stylistic - adjust as you tend to use
                        case "td": // stylistic - adjust as you tend to use
                            if ((options & PlainTextOptions.Table) != 0)
                            {
                                endElementString = "\t";
                            }

                            isInline = false;
                            break;

                        case "br":
                            if ((options & PlainTextOptions.LineFeed) != 0)
                            {
                                outText.Write("\r\n");
                                textInfo.WritePrecedingWhiteSpace = false;
                            }

                            skip = true;
                            isInline = true;
                            break;

                        case "a":
                            if ((options & PlainTextOptions.Link) != 0)
                            {
                                if (node.Attributes.Contains("href"))
                                {
                                    var href = node.Attributes["href"].Value.Trim();
                                    if (node.InnerText.IndexOf(href, StringComparison.OrdinalIgnoreCase) == -1)
                                    {
                                        endElementString = "<" + href + ">";
                                    }
                                }
                            }

                            isInline = true;
                            break;

                        case "li":
                            if ((options & PlainTextOptions.List) != 0)
                            {
                                if (textInfo.ListIndex > 0)
                                {
                                    outText.Write("{0}.\t", textInfo.ListIndex++);
                                }
                                else
                                {
                                    // using '•' as bullet char, with tab after, but whatever you want eg "\t->", if utf-8 0x2022
                                    outText.Write("•\t");
                                }

                                if (node.ChildNodes != null && node.ChildNodes.Any() &&
                                    node.ChildNodes.First().Name != "p")
                                {
                                    endElementString = "\r\n";
                                }
                            }

                            isInline = false;
                            break;

                        case "ol":
                            listIndex = 1;
                            goto case "ul";

                        // not handling nested lists any differently at this stage - that is getting close to rendering problems
                        case "ul":
                            if ((options & PlainTextOptions.List) != 0)
                            {
                                AddNewLineAtStartIfRequired(node, outText, textInfo);

                                if (!IsLastChildNodeInSpecialNodes(node))
                                {
                                    endElementString = "\r\n";
                                    textInfo.IsNewLineAddedAtEnd = true;
                                }
                            }

                            isInline = false;
                            break;

                        case "img": // inline-block in reality
                            if ((options & PlainTextOptions.Image) != 0)
                            {
                                outText.Write("[Image]");
                            }

                            isInline = true;
                            break;

                        case "pre":
                            textInfo.PreserveFormatting = true;
                            isInline = true;
                            break;

                        default:
                            isInline = true;
                            break;
                    }

                    if (!skip && node.HasChildNodes)
                    {
                        var domTextInfo = isInline
                            ? textInfo
                            : new DomTextInfo
                            {
                                ListIndex = listIndex,
                                IsNewLineAddedAtEnd = textInfo.IsNewLineAddedAtEnd
                            };

                        ConvertContentTo(node, outText, domTextInfo, options);
                    }

                    if (endElementString != null)
                    {
                        outText.Write(endElementString);
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(node.NodeType));
            }
        }

        private static string DecodeHtml(string text, bool verify = false)
        {
            if (verify && !IsHtmlEncoded(text))
            {
                return text;
            }

            return HtmlEntity.DeEntitize(text);
        }

        private static bool IsHtmlEncoded(string htmlText)
        {
            var input = htmlText ?? string.Empty;
            // strip out all possible attributes from html text (have to consider HTML tags only)
            var strippedText =
                Regex.Replace(input, "<([a-z][a-z0-9]*)[^>]*?(\\/?)>", "<$1$2>", RegexOptions.IgnoreCase);
            // ignore inner texts
            strippedText = Regex.Replace(strippedText, "(>)(.*?)(<)", "$1$3", RegexOptions.IgnoreCase);

            // decode the result and return true if text has changed
            return !HtmlEntity.DeEntitize(strippedText).Equals(strippedText);
        }

        private static void AddNewLineAtStartIfRequired(HtmlNode node, TextWriter outText, DomTextInfo textInfo)
        {
            // The new line before the paragraph, it it's the first child in the predefined nodes.
            if (node.ParentNode != null
                && !SpecialNodeNames.Contains(node.ParentNode.Name)
                && node.PreviousSibling != null
                && !textInfo.IsNewLineAddedAtEnd)
            {
                outText.Write("\r\n");
            }
        }

        private static bool IsLastChildNodeInSpecialNodes(HtmlNode node)
        {
            var parentNode = node.ParentNode;

            return parentNode != null &&
                   SpecialNodeNames.Contains(parentNode.Name) &&
                   node.NextSibling == null;
        }
    }
}
