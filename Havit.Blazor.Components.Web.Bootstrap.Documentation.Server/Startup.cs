using System;
using System.Globalization;
using Havit.Blazor.Components.Web.Bootstrap.Documentation.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Havit.Blazor.Components.Web.Bootstrap.Documentation.Server
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRazorPages();

			services.AddHxServices();
			services.AddHxMessenger();
			services.AddHxMessageBoxHost();

			services.AddTransient<IComponentApiDocModelBuilder, ComponentApiDocModelBuilder>();
			services.AddSingleton<IDocXmlProvider, DocXmlProvider>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			var cultureInfo = new CultureInfo("en-US");
			CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
			CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseWebAssemblyDebugging();
			}
			else
			{
				app.UseExceptionHandler("/Error");

				// old domain redirect
				app.Use(async (context, next) =>
				{

					if (context.Request.Host.Host.Contains("havit.blazor.cz"))
					{
						var uriBuilder = new UriBuilder(UriHelper.GetDisplayUrl(context.Request));
						uriBuilder.Host = "havit.blazor.eu";
						context.Response.Redirect(uriBuilder.Uri.ToString(), permanent: true);

						return;
					}

					await next();
				});
			}

			app.UseBlazorFrameworkFiles();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapRazorPages();
				endpoints.MapControllers();
				endpoints.MapFallbackToPage("/_Host");
			});
		}
	}
}