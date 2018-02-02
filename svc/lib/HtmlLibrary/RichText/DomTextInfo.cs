namespace HtmlLibrary.RichText
{
    internal class DomTextInfo
    {
        public bool WritePrecedingWhiteSpace { get; set; }

        public bool LastCharWasSpace { get; set; }

        public bool PreserveFormatting { get; set; }

        public int ListIndex { get; set; }

        public bool IsNewLineAddedAtEnd { get; set; }
    }
}
