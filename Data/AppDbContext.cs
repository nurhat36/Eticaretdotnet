using Microsoft.EntityFrameworkCore;
using ETicaret.Models;  // Modeli kullanabilmek için gerekli

namespace ETicaret.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet'ler burada tanýmlanýr
        public DbSet<Product> Products { get; set; }
    }
}

