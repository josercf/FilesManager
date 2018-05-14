using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FilesManager.Storage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FilesManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var storageAccount = Configuration["Blob_StorageAccount"];
            var storageKey = Configuration["Blob_StorageKey"];
            var containerName = Configuration["Blob_ContainerName"];

            var settings = new AzureBlobSetings(storageAccount, storageKey, containerName);

            services.AddScoped<IAzureBlobStorage>(factory =>
            {
                return new AzureBlobStorage(settings);
            });

            services.AddTransient<AzureTableStorage, AzureTableStorage>(factory =>
            {
                return new AzureTableStorage(settings);
            } );

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                   .AddCookie(options =>
                   {
                       options.LoginPath = "/Login/Fail";
                   });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Login");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Login}/{action=Index}/{id?}");
            });
        }
    }
}
