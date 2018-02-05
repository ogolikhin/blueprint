using System;

namespace HtmlLibrary.RichText
{
    [Flags]
    public enum PlainTextOptions
    {
        None,
        Verify = 1,       // Validate HTML
        LineFeed = 2,     // Transform a line feed
        Link = 4,         // Convert links (<a href="" />) to text like "text <URL>"
        Image = 8,        // Convert images (<img alt="" src="" />) to "[Image]"
        Table = 16,       // Convert tables to text using "\t(TAB)" as cell separator
        List = 32,        // Convert list elements to text using  "\t(TAB)" as separator
        Paragraph = 64,   // Convert paragraph with a line feed
        Entitize = 128,   // Entitize (decode) text
        TextOnly = LineFeed | Verify,
        All = Verify | LineFeed | Table | List | Paragraph | Entitize
    }
}
