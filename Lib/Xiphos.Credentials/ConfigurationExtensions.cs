using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Xiphos.Credentials
{
    /// <summary>
    /// Service collection extensions
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Registers development implementation of <see cref="IServiceCredentialStore"/>
        /// </summary>
        /// <param name="serviceCollection">Extended service collection</param>
        /// <returns>Extended service collection</returns>
        public static IServiceCollection AddDevelopmentServiceCredentials(this IServiceCollection serviceCollection)
        {
            // Bind the configuration section with credentials.
            serviceCollection
                .AddOptions<DevelopmentServiceCredentialStoreOptions>()
                .Configure<IConfiguration>((options, configuration) =>
                    configuration.Bind(DevelopmentServiceCredentialStoreOptions.SectionName, options));

            // Register the service
            serviceCollection.AddSingleton<IServiceCredentialStore, DevelopmentServiceCredentialStore>();

            return serviceCollection;
        }

        /// <summary>
        /// Registers production implementation of <see cref="IServiceCredentialStore"/>
        /// </summary>
        /// <param name="serviceCollection">Extended service collection</param>
        /// <returns>Extended service collection</returns>
        public static IServiceCollection AddProductionServiceCredentials(this IServiceCollection serviceCollection)
            => serviceCollection.AddSingleton<IServiceCredentialStore, ProductionServiceCredentialStore>();
    }
}
