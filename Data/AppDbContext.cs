using Microsoft.EntityFrameworkCore;
using ETicaret.Models;  // Modeli kullanabilmek i�in gerekli

namespace ETicaret.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet'ler burada tan�mlan�r
        public DbSet<Product> Products { get; set; }
    }
}

