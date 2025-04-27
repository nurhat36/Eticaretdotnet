using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ETicaret.Models;  // User modelini kullanabilmek için gerekli

namespace ETicaret.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>  // User sýnýfýný kullanýyoruz
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet'ler burada tanýmlanýr
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }  // User tablosu için doðru DbSet

        // Veritabaný modeli burada özelleþtirilir
    }
}
