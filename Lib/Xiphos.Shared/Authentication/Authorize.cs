namespace Xiphos.Shared.Authentication
{
    /// <summary>
    /// Authorization roles
    /// </summary>
    public static class Authorize
    {
        /// <summary>
        /// Administrator only
        /// </summary>
        public const string Administrator = UserRoles.Administrator;

        /// <summary>
        /// User or administrator
        /// </summary>
        public const string UserOrAdministrator = UserRoles.User + "," + UserRoles.Administrator;
    }
}
