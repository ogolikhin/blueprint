using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AdminStore.Models;

namespace AdminStore
{
    internal class UnitTestHelper
    {
        public class FakeResponseHandler : DelegatingHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _sendFunc;

            public FakeResponseHandler(Func<HttpRequestMessage, HttpResponseMessage> sendFunc)
            {
                _sendFunc = sendFunc;
            }

            protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return await Task.Run(() => _sendFunc(request), cancellationToken);
            }
        }

        internal class FederatedAuthenticationSettingsEqualityComparer : EqualityComparer<IFederatedAuthenticationSettings>
        {
            public override bool Equals(IFederatedAuthenticationSettings x, IFederatedAuthenticationSettings y)
            {
                if (!Equals(x.Certificate, y.Certificate))
                {
                    return false;
                }
                if (!Equals(x.ErrorUrl, y.ErrorUrl))
                {
                    return false;
                }
                if (!Equals(x.LoginUrl, y.LoginUrl))
                {
                    return false;
                }
                if (!Equals(x.LogoutUrl, y.LogoutUrl))
                {
                    return false;
                }
                if (!Equals(x.NameClaimType, y.NameClaimType))
                {
                    return false;
                }
                return true;
            }

            public override int GetHashCode(IFederatedAuthenticationSettings obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
