namespace Xiphos.Credentials
{
    /// <summary>
    /// Development-only service credential configuration.
    /// </summary>
    public class DevelopmentServiceCredentialStoreOptions
    {
        /// <summary>
        /// Configuration section name
        /// </summary>
        public static readonly string SectionName = "ServiceCredentialStore";

        /// <summary>
        /// SQL connection string the service database
        /// </summary>
        public string ServiceDatabaseConnectionString { get; set; }

        /// <summary>
        /// SQL connection string to the product database
        /// </summary>
        public string ProductDatabaseConnectionString { get; set; }
    }
}
