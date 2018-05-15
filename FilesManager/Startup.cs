using FilesManager.DataAccess.Storage;
using FilesManager.DataAccess.Storage.BusinessContracts;
using FilesManager.DataAccess.Storage.Contracts;
using FilesManager.DataAccess.Storage.Infraestructure;
using FilesManager.DataAccess.Storage.Models;
using FilesManager.DataAccess.Storage.Services;
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

            var _settings = new StorageAccountSettings(storageAccount, storageKey, containerName);

            services.AddTransient<StorageAccountSettings>(provider => { return _settings; });
            services.AddTransient<IAzureBlobStorage, AzureBlobStorage>();
            services.AddTransient(typeof(IAzureTableStorage<>), typeof(AzureTableStorage<>));

            services.AddTransient<IUserService, UserService>((ctx) =>
            {
                IAzureTableStorage<User> svc = ctx.GetService<IAzureTableStorage<User>>();
                return new UserService(svc);
            });


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
