namespace Xiphos.Credentials
{
    /// <summary>
    /// Read only credential store abstraction for service level access.
    /// </summary>
    public interface IServiceCredentialStore
    {
        /// <summary>
        /// Provides an R/W connection string to the service database.
        /// </summary>
        /// <returns>Connection string</returns>
        public string GetServiceDatabaseConnectionString();

        /// <summary>
        /// Provides an R/W connection string to the product database.
        /// </summary>
        /// <returns>Connection string</returns>
        public string GetProductDatabaseConnectionString();
    }
}
