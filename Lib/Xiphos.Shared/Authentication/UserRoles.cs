using System.Collections.Generic;

namespace Xiphos.Shared.Authentication
{
    /// <summary>
    /// User role types
    /// </summary>
    public static class UserRoles
    {
        /// <summary>
        /// Regular logged in user
        /// </summary>
        public const string User = nameof(User);

        /// <summary>
        /// Application administrator with the keys to the kingdom
        /// </summary>
        public const string Administrator = nameof(Administrator);

        /// <summary>
        /// Set of all roles
        /// </summary>
        public static IEnumerable<string> AllRoles => new[] { User, Administrator };
    }
}
