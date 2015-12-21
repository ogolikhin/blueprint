using System;

namespace Model.Impl
{
    public class Attachment : IAttachment
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Link { get; set; }
        public bool IsReadOnly { get; set; }
    }
}
