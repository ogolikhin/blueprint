using System.Collections.Generic;
using Model.OpenApiModel;

namespace Model
{
    public interface IDiscussion
    {
        List<IComment> Comments { get; }
    }
}
