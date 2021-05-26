using Microsoft.Extensions.Options;
using System;

namespace Xiphos.Credentials
{
    /// <summary>
    /// Development-only service credential source.
    /// </summary>
    public class DevelopmentServiceCredentialStore : IServiceCredentialStore
    {
        private readonly DevelopmentServiceCredentialStoreOptions _options;

        /// <summary>
        /// Ctr.
        /// </summary>
        /// <param name="configuration">Credential store options</param>
        public DevelopmentServiceCredentialStore(IOptions<DevelopmentServiceCredentialStoreOptions> configuration)
        {
            _options = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrEmpty(_options.ServiceDatabaseConnectionString))
                throw new ArgumentNullException("Missing mandatory configuration: " +
                    $"{DevelopmentServiceCredentialStoreOptions.SectionName}:{nameof(_options.ServiceDatabaseConnectionString)}");
        }

        /// <inheritdoc cref="IServiceCredentialStore"/>
        public string GetServiceDatabaseConnectionString()
            => _options.ServiceDatabaseConnectionString;

        /// <inheritdoc cref="IServiceCredentialStore"/>
        public string GetProductDatabaseConnectionString()
            => _options.ProductDatabaseConnectionString;
    }
}
