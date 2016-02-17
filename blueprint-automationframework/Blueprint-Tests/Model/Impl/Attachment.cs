using System;

namespace Model.Impl
{
    public class Attachment : IAttachment
    {
        // TODO Future development
    }

    public class OpenApiAttachment : Attachment, IOpenApiAttachment
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Link { get; set; }
        public bool IsReadOnly { get; set; }
    }
}
