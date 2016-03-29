using System;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Repositories
{
    public class ServiceDependencyStatusRepository : IStatusRepository
    {
        public string Name { get; set; }

        private readonly HttpClientProvider _httpClientProvider;
        private readonly Uri _serviceUri;

        public ServiceDependencyStatusRepository(HttpClientProvider httpClientProvider, Uri serviceUri, string name)
        {
            _httpClientProvider = httpClientProvider;
            _serviceUri = serviceUri;
            Name = name;
        }

        public async Task<string> GetStatus()
        {
            var serviceHttpClient = _httpClientProvider.Create(_serviceUri);
            var result = await serviceHttpClient.GetAsync("status/upcheck");
            return await result.Content.ReadAsStringAsync();
        }
    }
}
