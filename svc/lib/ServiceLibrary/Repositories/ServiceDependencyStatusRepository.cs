﻿using ServiceLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public class ServiceDependencyStatusRepository : IStatusRepository
    {
        private readonly Uri _requestUri;

        public string Name { get; set; }

        public string AccessInfo { get; set; }

        public ServiceDependencyStatusRepository(Uri serviceUri, string name)
        {
            _requestUri = new Uri(serviceUri, "status/upcheck");
            Name = name;
            AccessInfo = _requestUri.ToString();
        }

        private async Task<StatusResponse> GetStatus(int timeout)
        {
            var webRequest = WebRequest.CreateHttp(_requestUri);
            webRequest.Timeout = timeout;
            HttpWebResponse result = (HttpWebResponse)await webRequest.GetResponseAsync();
            var responseData = new StatusResponse
            {
                Name = Name,
                AccessInfo = AccessInfo,
                Result = ((int)result.StatusCode).ToString(),
                NoErrors = true
            };

            return responseData;
        }

        public async Task<List<StatusResponse>> GetStatuses(int timeout)
        {
            return new List<StatusResponse> { await GetStatus(timeout) };
        }
    }
}
