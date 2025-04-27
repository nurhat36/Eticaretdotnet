using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ETicaret.Models;  // User modelini kullanabilmek i�in gerekli

namespace ETicaret.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>  // User s�n�f�n� kullan�yoruz
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet'ler burada tan�mlan�r
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }  // User tablosu i�in do�ru DbSet

        // Veritaban� modeli burada �zelle�tirilir
    }
}
