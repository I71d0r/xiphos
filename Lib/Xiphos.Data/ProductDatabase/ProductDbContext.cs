using Microsoft.EntityFrameworkCore;
using Xiphos.Data.Models;

namespace Xiphos.Data.ProductDatabase
{
    public class ProductDbContext : DbContext
    {
        public DbSet<MelodyModel> Melodies { get; set; }

        public ProductDbContext(DbContextOptions<ProductDbContext> options)
            : base(options)
        {
        }
    }
}
