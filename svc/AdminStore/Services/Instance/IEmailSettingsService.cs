using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Models.Emails;

namespace AdminStore.Services.Instance
{
    public interface IEmailSettingsService
    {
        Task SendTestEmailAsync(int userId, EmailOutgoingSettings outgoingSettings);
    }
}
