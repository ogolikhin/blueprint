﻿using Newtonsoft.Json;
using ServiceLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
	public class ServiceConfigRepository : IConfigRepository
	{
		internal readonly IHttpClientProvider _httpClientProvider;
		public ServiceConfigRepository()
			: this(new HttpClientProvider())
		{
		}

		internal ServiceConfigRepository(IHttpClientProvider hcp)
		{
			_httpClientProvider = hcp;
		}

		public async Task<Dictionary<string, Dictionary<string, string>>> GetConfig()
		{
			var uri = ConfigurationManager.AppSettings["ConfigControl"] + "true";
         using (var http = _httpClientProvider.Create())
			{
				http.BaseAddress = new Uri(uri);
				http.DefaultRequestHeaders.Accept.Clear();
				var result = await http.GetAsync("settings");
				result.EnsureSuccessStatusCode();
				return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(await result.Content.ReadAsStringAsync());
			}
		}
	}
}
