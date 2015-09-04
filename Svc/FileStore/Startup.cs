using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;

namespace FileStore
{
	public class Startup
	{
		public static IConfiguration Configuration { get; set; }
		public static string FileStoreDatabaseConnectionString { get; set; }

		public Startup(IHostingEnvironment env)
		{
			Configuration = new Configuration().AddJsonFile("config.json").AddEnvironmentVariables();
			FileStoreDatabaseConnectionString = Configuration.Get("Data:Databases:FileStoreDatabase");
		}

		// This method gets called by a runtime.
		// Use this method to add services to the container
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();
			// Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
			// You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
			// services.AddWebApiConventions();
		}

		// Configure is called after ConfigureServices is called.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			// Configure the HTTP request pipeline.
			app.UseStaticFiles();

			// Add MVC to the request pipeline.
			app.UseMvc();
			// Add the following route for porting Web API 2 controllers.
			// routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
		}
	}
}
