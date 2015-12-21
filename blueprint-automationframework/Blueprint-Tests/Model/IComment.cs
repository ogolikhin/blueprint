using System;
using System.Collections.Generic;
using Model.Impl;

namespace Model
{
    public interface IComment
    {
        string LastModified { get; set; }
        bool IsClosed { get; set; }
        string Status { get; set; }
        int Id { get; set; }
        IAuthor Author { get; set; }
        int Version { get; set; }
        string Description { get; set; }
    }
}
