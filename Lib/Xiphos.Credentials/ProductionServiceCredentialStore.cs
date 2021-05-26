using System;

namespace Xiphos.Credentials
{
    /// <summary>
    /// Production credential store.
    /// The implementation encapsulates deployment specifics.
    /// </summary>
    public class ProductionServiceCredentialStore : IServiceCredentialStore
    {
        /// <inheritdoc cref="IServiceCredentialStore"/>
        public string GetServiceDatabaseConnectionString()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IServiceCredentialStore"/>
        public string GetProductDatabaseConnectionString()
            => throw new NotImplementedException();
    }
}
