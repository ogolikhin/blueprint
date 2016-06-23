using ServiceLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public class SqlInstanceSettingsRepositoryMock: IInstanceSettingsRepository
    {
        private EmailSettings _mockEmailSettings;

        public SqlInstanceSettingsRepositoryMock(EmailSettings emailSettings) {
            _mockEmailSettings = emailSettings;
        }

        public async Task<EmailSettings> GetEmailSettings()
        {
            return await Task.FromResult(_mockEmailSettings);
        }
    }
}
