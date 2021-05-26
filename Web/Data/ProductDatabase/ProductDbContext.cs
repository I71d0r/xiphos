using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xiphos.Areas.Administration.Models;

namespace Xiphos.Data
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
