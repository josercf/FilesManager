using FaispCertifieldProcess.DependencyInjection;
using FilesManager.DataAccess.Storage;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.DependencyInjection;

namespace FaispCertifieldProcess
{
    public class InjectConfiguration : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            var services = new ServiceCollection();
            RegisterServices(services);
            var serviceProvider = services.BuildServiceProvider(true);

            context
                .AddBindingRule<InjectAttribute>()
                .Bind(new InjectBindingProvider(serviceProvider));

            var registry = context.Config.GetService<IExtensionRegistry>();
            var filter = new ScopeCleanupFilter();
            registry.RegisterExtension(typeof(IFunctionInvocationFilter), filter);
            registry.RegisterExtension(typeof(IFunctionExceptionFilter), filter);
        }
        private void RegisterServices(IServiceCollection services)
        {
            var storageAccount = "cosmoshoroscopob34c";
            var storageKey = "ortdcoPVj90rv0GyPGDzMN/jN+5K0izumxFbIqvRM6MiDcXQwcNLSJomGeDQE3RdfhowIyH9MQw856fikwiIrw==";
            var containerName = "teste";

            var settings = new StorageAccountSettings(storageAccount, storageKey, containerName);
            services.AddTransient<StorageAccountSettings>(provider =>  { return settings; });

            services.AddScoped<IGreeter, Greeter>();
        }
    }
}
