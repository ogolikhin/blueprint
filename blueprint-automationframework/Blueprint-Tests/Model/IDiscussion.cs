using System.Collections.Generic;

namespace Model
{
    public interface IDiscussion
    {
        List<IComment> Comments { get; }
    }
}
