using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminStore.Services.Email
{
    public interface IEmailClient : IDisposable
    {
        bool UseSsl { get; set; }

        void Connect(string serverAddress, int port);

        void Login(string userName, string password);

        void Disconnect();
    }
}
