using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlLibrary.Tests
{
    public class MentionHelperMock : IMentionValidator
    {
        public async Task<bool> IsEmailBlocked(string email)
        { 
            if (email == "blocked@blocked.com")
            {
                return (await Task.FromResult(true));
            }
            return (await Task.FromResult(false));
        }
    }
}
