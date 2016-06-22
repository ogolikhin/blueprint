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
            await Task.Run(() => { });
            return new EmailSettings { Id = "Fake",
                                       Authenticated = true,
                                       Domains = "FakeDomain",
                                       EnableAllUsers = true,
                                       EnableDomains = true,
                                       EnableEmailDiscussion = true,
                                       EnableEmailReplies = true,
                                       EnableNotifications = true,
                                       EnableSSL = true,
                                       HostName = "FakeHostName",
                                       IncomingEnableSSL = true,
                                       IncomingHostName = "FakeIncomingHostName",
                                       IncomingPassword = "FakeIncomingPassword",
                                       IncomingPort = 1234,
                                       IncomingServerType = 1,
                                       IncomingUserName = "FakeIncomingUserName",
                                       Password = "FakePassword",
                                       Port = 1234,
                                       SenderEmailAddress = "FakeSenderAddress",
                                       UserName = "FakeUserName"
            };

        }
    }
}
