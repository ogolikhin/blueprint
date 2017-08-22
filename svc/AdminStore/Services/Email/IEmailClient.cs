using System;

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
