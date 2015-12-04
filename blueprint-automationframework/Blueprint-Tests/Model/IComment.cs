using System;
using System.Collections.Generic;
using Model.Impl;

namespace Model
{
    public interface IComment
    {
        string Comment { get; set; }
        int Dislikes { get; set; }
        bool IsOpen { get; set; }
        int Likes { get; set; }
        List<IComment> Replies { get; }
        DateTime Timestamp { get; set; }
        User User { get; set; }
    }
}
