using System.Collections.Generic;

namespace ServiceLibrary.Models
{
    public interface IPaginatedResult<T>
    {
        int Total { get; set; }

        int Count { get; set; }

        IEnumerable<T> Items { get; set; }
    }
}