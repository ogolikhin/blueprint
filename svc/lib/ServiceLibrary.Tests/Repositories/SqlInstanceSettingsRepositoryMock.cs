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
        public async Task<EmailSettings> GetEmailSettings()
        {
            return await Task.FromResult(new EmailSettings
            {
                Id = "Fake",
                Authenticated = false,
                Domains = "FakeDomain",
                EnableAllUsers = false,
                EnableDomains = false,
                EnableEmailDiscussion = false,
                EnableEmailReplies = false,
                EnableNotifications = false,
                EnableSSL = false,
                HostName = "FakeHostName",
                IncomingEnableSSL = false,
                IncomingHostName = "FakeIncomingHostName",
                IncomingPassword = "FakeIncomingPassword",
                IncomingPort = 1234,
                IncomingServerType = 1,
                IncomingUserName = "FakeIncomingUserName",
                Password = "FakePassword",
                Port = 1234,
                SenderEmailAddress = "FakeSenderAddress",
                UserName = "FakeUserName"
            });

        }
    }
}
