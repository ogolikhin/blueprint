using System.Collections.Generic;

namespace AdminStore.Models
{
    public class Data
    {
        public List<UserDto> Users { get; private set; }

        public Data()
        {
            Users = new List<UserDto>();
        }
    }
}