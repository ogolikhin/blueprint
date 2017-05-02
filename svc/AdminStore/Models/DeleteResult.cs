using System.Collections.Generic;

namespace AdminStore.Models
{
    public class DeleteResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public IEnumerable<int> Ids { get; set; } 
    }
}