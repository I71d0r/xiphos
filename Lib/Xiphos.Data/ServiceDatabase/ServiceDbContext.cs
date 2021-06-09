using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Xiphos.Data.ServiceDatabase
{
    //todo: extend user with a settings table, use IdentityDbContext<TUser>
    public class ServiceDbContext : IdentityDbContext
    {
        public ServiceDbContext(DbContextOptions<ServiceDbContext> options)
            : base(options)
        {
        }
    }
}
